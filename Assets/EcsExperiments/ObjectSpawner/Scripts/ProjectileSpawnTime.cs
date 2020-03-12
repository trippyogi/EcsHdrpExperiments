using System;
using Unity.Entities;

namespace Advanced.ObjectSpawner
{
    [Serializable]
    public struct ProjectileSpawnTime : IComponentData
    {
        public float SpawnTime;
    }
}
