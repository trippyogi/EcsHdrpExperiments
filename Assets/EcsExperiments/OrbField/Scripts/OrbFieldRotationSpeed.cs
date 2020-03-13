using System;
using Unity.Entities;

// ReSharper disable once InconsistentNaming
[Serializable]
public struct OrbFieldRotationSpeed : IComponentData
{
    public float RadiansPerSecond;
}
