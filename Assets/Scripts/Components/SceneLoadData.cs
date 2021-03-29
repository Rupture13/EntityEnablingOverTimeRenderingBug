using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct SceneLoadData : IComponentData
{
    public bool IsProcessing;
    public int ProcessingIndex;
}
