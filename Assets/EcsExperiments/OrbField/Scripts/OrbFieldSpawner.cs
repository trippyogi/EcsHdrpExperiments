using Unity.Entities;

// ReSharper disable once InconsistentNaming
public struct OrbFieldSpawner : IComponentData
{
    public int CountX;
    public int CountY;
    public Entity Prefab;
}
