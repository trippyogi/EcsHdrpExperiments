using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Samples.Boids
{
    [Serializable]
    [MaterialProperty("_BoidEmission", MaterialPropertyFormat.Float)]
    public struct EmissionComponent : IComponentData
    {
        public float Value;
    }

    namespace Authoring
    {
        [DisallowMultipleComponent]
        [RequiresEntityConversion]
        [ConverterVersion("jeremy", 1)]
        public class EmissionComponent : MonoBehaviour, IConvertGameObjectToEntity
        {
            public float intensity;

            public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            {
                var data = new Samples.Boids.EmissionComponent { Value = intensity };
                dstManager.AddComponentData(entity, data);
            }
        }
    }
}