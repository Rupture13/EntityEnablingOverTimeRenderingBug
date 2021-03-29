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

        var sceneEntities = m_sceneQuery.ToEntityArray(Allocator.Temp);
        var sceneEntity = sceneEntities.Length == 1 ? sceneEntities[0] : Entity.Null;

        m_sceneSystem.LoadSceneAsync(sceneEntity);
        sceneEntities.Dispose();

        Entities.ForEach((ref SceneLoadData loaderData) =>
        {
            loaderData.IsProcessing = true;
            loaderData.ProcessingIndex = -1;
        }).Schedule();
    }

    protected override void OnUpdate()
    {
        if (m_toEnableEntityGroupsQuery.CalculateEntityCount() == 0) { return; }

        var loadingStatus = GetSingleton<SceneLoadData>();
        var processingIndex = loadingStatus.ProcessingIndex;
        var isProcessing = loadingStatus.IsProcessing;

        if (isProcessing && processingIndex == -1)
        {
            Debug.Log("Preworky");
            m_toEnableEntities = m_toEnableEntityGroupsQuery.ToEntityArray(Allocator.Persistent);

            processingIndex = 0;
        }


        if (isProcessing && processingIndex > -1)
        {
            var e = m_toEnableEntities[processingIndex];

            EntityManager.SetEnabled(e, true);
            Debug.LogWarning($"Processing progress: {processingIndex + 1} of {m_toEnableEntities.Length}.\nEntity {e.Index}v{e.Version}");

            processingIndex = (processingIndex + 1);

            if (math.abs(processingIndex) == m_toEnableEntities.Length)
            {
                Debug.Log("Processing done");
                processingIndex = -1;
                isProcessing = false;

                m_toEnableEntities.Dispose();
            }
        }

        SetSingleton(new SceneLoadData { IsProcessing = isProcessing, ProcessingIndex = processingIndex });
    }
}
