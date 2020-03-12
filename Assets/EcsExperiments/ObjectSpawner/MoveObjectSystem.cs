using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Advanced.ObjectSpawner
{
    // This system updates all entities in the scene with both a RotationSpeed and Rotation component.
    public class MoveObjectSystem : JobComponentSystem
    {
        public NativeArray<float> GlobalFftArray;

        [BurstCompile]
        struct MoveObjectJob : IJobForEachWithEntity<ProjectileSpawnTime, LocalToWorld>
        {
            public EntityCommandBuffer.Concurrent Commands;
            public float TimeSinceLoad;
            public float ProjectileSpeed;

            // [NativeDisableParallelForRestriction]
            [ReadOnly] public NativeArray<float> FftArray;

            public void Execute(Entity entity, int index, [ReadOnly] ref ProjectileSpawnTime spawnTime,
                ref LocalToWorld localToWorld)
            {
                float aliveTime = (TimeSinceLoad - spawnTime.SpawnTime);
                if (aliveTime > 5.0f)
                {
                    Commands.DestroyEntity(index, entity);
                }
                
                var fftIndex = index % 11;
                var translation = new float3(aliveTime * ProjectileSpeed,
                    localToWorld.Position.y,
                    localToWorld.Position.z);
                var rotation = Quaternion.LookRotation(localToWorld.Forward, localToWorld.Up);
                var scale = FftArray[fftIndex];

                // translation.Value.x = aliveTime * ProjectileSpeed + scale * 10f;

                // var xPosition = localToWorld.Position.x;
                localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(
                        translation,
                        rotation,
                        new float3(10 * scale, 10 * scale, 10 * scale))
                };
            }
        }

        BeginSimulationEntityCommandBufferSystem m_beginSimEcbSystem;

        protected override void OnCreate()
        {
            m_beginSimEcbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var jobHandle = new MoveObjectJob()
            {
                Commands = m_beginSimEcbSystem.CreateCommandBuffer().ToConcurrent(),
                TimeSinceLoad = (float) Time.ElapsedTime,
                ProjectileSpeed = 5.0f,
                FftArray = GlobalFftArray
            }.Schedule(this, inputDependencies);
            m_beginSimEcbSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}