using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[RequiresEntityConversion]
[AddComponentMenu("DOTS Samples/Orb Field")]
[ConverterVersion("jeremy", 1)]
public class OrbFieldAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float DegreesPerSecond = 360;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new OrbFieldRotationSpeed { RadiansPerSecond = math.radians(DegreesPerSecond) });
        dstManager.AddComponentData(entity, new IndexComponent() { Index = Random.Range(0, 11) });
    }
}
