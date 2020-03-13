using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("DOTS Samples/EyeLasers/Spawner")]
[ConverterVersion("joe", 1)]
public class OrbFieldSpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject Prefab;
    public int CountX;
    public int CountY;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new OrbFieldSpawner
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            CountX = CountX,
            CountY = CountY,
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}

