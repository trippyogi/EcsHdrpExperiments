using System;
using Unity.Entities;

[Serializable]
public struct OrbFieldRotationSpeed : IComponentData
{
    public float RadiansPerSecond;
}
