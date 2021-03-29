using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class ToLoadSceneAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //Rather than adding runtime ToLoadSceneTag component to this entity, 
        //find the corresponding subscene entity and add the component to that instead
        Entity sceneEntity = conversionSystem.GetSceneSectionEntity(entity);
        dstManager.AddComponentData(sceneEntity, new ToLoadSceneTag());
    }
}
