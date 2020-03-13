using Unity.Entities;

public struct OrbFieldSpawner : IComponentData
{
    public int CountX;
    public int CountY;
    public Entity Prefab;
}
