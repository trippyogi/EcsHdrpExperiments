using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
                        Value = new float3(0, 0.9f * math.sin(5.0f * spawnTime), 0.9f * math.cos(5.0f * spawnTime))
                    });
                PostUpdateCommands.SetComponent(newEntity, new ProjectileSpawnTime { SpawnTime = spawnTime });
                PostUpdateCommands.AddComponent(newEntity, new Scale { Value = 0 });
            });
        }
    }
}