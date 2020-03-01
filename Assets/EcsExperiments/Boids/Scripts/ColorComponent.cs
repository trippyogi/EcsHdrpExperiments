using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Samples.Boids
{
    [Serializable]
    [MaterialProperty("_BoidColor", MaterialPropertyFormat.Float4)]
    public struct ColorComponent : IComponentData
    {
        public float4 Value;
    }

    namespace Authoring
    {
        [DisallowMultipleComponent]
        [RequiresEntityConversion]
        [ConverterVersion("joe", 1)]
        public class ColorComponent : MonoBehaviour, IConvertGameObjectToEntity
        {
            public Color color;

            public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            {
                var data = new Samples.Boids.ColorComponent { Value = new float4(color.r, color.g, color.b, color.a) };
                dstManager.AddComponentData(entity, data);
            }
        }
    }
}

// [Serializable]
// [MaterialProperty("_Emission", MaterialPropertyFormat.Float4)]
// public struct MaterialEmission : IComponentData
// {
//     public float4 Value;
// }