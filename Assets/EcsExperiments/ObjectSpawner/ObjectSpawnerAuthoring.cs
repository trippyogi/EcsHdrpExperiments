using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Advanced.ObjectSpawner
{
    [AddComponentMenu("DOTS Samples/FixedTimestepWorkaround/Object Spawner")]
    [ConverterVersion("joe", 1)]
    public class ObjectSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject projectilePrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new ObjectSpawner
            {
                Prefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
            };
            dstManager.AddComponentData(entity, spawnerData);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(projectilePrefab);
        }
    }
}
