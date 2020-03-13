using System;
using Unity.Entities;
using Unity.Transforms;

// ReSharper disable once InconsistentNaming
[GenerateAuthoringComponent]
public struct IndexComponent : IComponentData
{
    public int Index;
    public Translation InitialPosition;
}