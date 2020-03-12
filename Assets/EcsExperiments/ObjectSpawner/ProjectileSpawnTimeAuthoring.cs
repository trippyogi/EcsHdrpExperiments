using Unity.Entities;
using UnityEngine;

namespace Advanced.ObjectSpawner
{
    [RequiresEntityConversion]
    [AddComponentMenu("DOTS Samples/FixedTimestepWorkaround/Projectile Spawn Time")]
    [ConverterVersion("joe", 1)]
    public class ProjectileSpawnTimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float SpawnTime;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new ProjectileSpawnTime { SpawnTime = SpawnTime };
            dstManager.AddComponentData(entity, data);
        }
    }
}
