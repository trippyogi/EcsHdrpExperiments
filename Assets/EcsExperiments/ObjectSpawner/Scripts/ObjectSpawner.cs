using Samples.Boids.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Advanced.ObjectSpawner
{
    public struct ObjectSpawner : IComponentData
    {
        public Entity Prefab;
    }

    [DisableAutoCreation]
    public class ObjectSpawnerSystem : ComponentSystem
    {
        EntityQuery m_MainGroup;

        private readonly float4[] _colors =
        {
            new float4(106, 74, 232, 0),
            new float4(235, 66, 172, 0),
            new float4(255, 120, 120, 0),
            new float4(255, 152, 84, 0),
            new float4(255, 255, 105, 0),
            new float4(113, 245, 100, 0),
            new float4(95, 207, 232, 0)
        };

        protected override void OnCreate()
        {
            m_MainGroup = GetEntityQuery(
                ComponentType.ReadOnly<ObjectSpawner>(),
                ComponentType.ReadOnly<Translation>());
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity spawnerEntity, ref ObjectSpawner spawnerData, ref Translation translation) =>
            {
                var spawnTime = UnityEngine.Time.time;
                var newEntity = PostUpdateCommands.Instantiate(spawnerData.Prefab);
                // PostUpdateCommands.AddComponent(newEntity, new Parent { Value = spawnerEntity });
                // PostUpdateCommands.AddComponent(newEntity, new LocalToParent());
                PostUpdateCommands.SetComponent(newEntity,
                    new Translation
                    {
                        Value = new float3(0, 3f * math.sin(5.0f * spawnTime) + Random.Range(-3, 3),
                            3f * math.cos(5.0f * spawnTime) + Random.Range(-3, 3))
                    });
                PostUpdateCommands.SetComponent(newEntity, new ProjectileSpawnTime { SpawnTime = spawnTime });
                PostUpdateCommands.SetComponent(newEntity,
                    new Samples.Boids.ColorComponent { Value = _colors[Random.Range(0, 7)] });
                PostUpdateCommands.SetComponent(newEntity,
                    new Samples.Boids.EmissionComponent() { Value = 0 });
            });
        }
    }
}