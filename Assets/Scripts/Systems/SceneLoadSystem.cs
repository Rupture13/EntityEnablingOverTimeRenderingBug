using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

public class SceneLoadSystem : SystemBase
{
    private EntityQuery m_sceneQuery;
    private SceneSystem m_sceneSystem;

    private EntityQuery m_toEnableEntityGroupsQuery;
    private NativeArray<Entity> m_toEnableEntities;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_sceneQuery = GetEntityQuery(ComponentType.ReadOnly<ToLoadSceneTag>(), ComponentType.ReadOnly<SceneSectionData>());
        m_sceneSystem = World.GetExistingSystem<SceneSystem>();

        m_toEnableEntityGroupsQuery = GetEntityQuery(ComponentType.ReadOnly<Disabled>(), ComponentType.ReadOnly<LinkedEntityGroup>());

        RequireSingletonForUpdate<SceneLoadData>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        //Load subscene
        //Subscene is found by a tag that is addded to it in entity conversion time
        var sceneEntities = m_sceneQuery.ToEntityArray(Allocator.Temp);
        var sceneEntity = sceneEntities.Length == 1 ? sceneEntities[0] : Entity.Null;

        m_sceneSystem.LoadSceneAsync(sceneEntity);
        sceneEntities.Dispose();

        Entities.ForEach((ref SceneLoadData loaderData) =>
        {
            loaderData.IsProcessing = true;
            loaderData.ProcessingIndex = -1;
        }).Run();
    }

    protected override void OnUpdate()
    {
        //Don't do anything if there's no entities to enable
        if (m_toEnableEntityGroupsQuery.CalculateEntityCount() == 0) { return; }

        //Gather data from loadingstatus singleton
        var loadingStatus = GetSingleton<SceneLoadData>();
        var processingIndex = loadingStatus.ProcessingIndex;
        var isProcessing = loadingStatus.IsProcessing;

        //Retrieve list of entities to enable
        if (isProcessing && processingIndex == -1)
        {
            m_toEnableEntities = m_toEnableEntityGroupsQuery.ToEntityArray(Allocator.Persistent);

            processingIndex = 0;
        }

        //Enable entity of current processingindex
        //(using EntityManager.SetEnabled, so entities in the LinkedEntityGroup will be enabled too)
        if (isProcessing && processingIndex > -1)
        {
            var e = m_toEnableEntities[processingIndex];

            EntityManager.SetEnabled(e, true);
            Debug.LogWarning($"Processing progress: {processingIndex + 1} of {m_toEnableEntities.Length}.\nEntity {e.Index}v{e.Version}");

            ++processingIndex;

            //When all to-enable entities are processed,
            //set data to stop the system from trying to process any more
            //and clean up NativeArray
            if (processingIndex == m_toEnableEntities.Length)
            {
                Debug.Log("Processing done");
                processingIndex = -1;
                isProcessing = false;

                m_toEnableEntities.Dispose();
            }
        }

        //Set possibly modified loadingstatus data
        SetSingleton(new SceneLoadData { IsProcessing = isProcessing, ProcessingIndex = processingIndex });
    }
}
