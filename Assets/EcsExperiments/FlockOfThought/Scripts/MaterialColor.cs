using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[Serializable]
[MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
public struct MaterialColor : IComponentData
{
    public float4 Value;
}

[Serializable]
[MaterialProperty("_Emission", MaterialPropertyFormat.Float4)]
public struct MaterialEmission : IComponentData
{
    public float4 Value;
}
