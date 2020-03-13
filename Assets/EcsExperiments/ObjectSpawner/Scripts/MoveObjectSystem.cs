using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Advanced.ObjectSpawner
{
    public class MoveObjectSystem : JobComponentSystem
    {
        [DeallocateOnJobCompletion] [NativeDisableParallelForRestriction]
        public NativeArray<float> GlobalFftArray;// = new NativeArray<float>(11, Allocator.TempJob);

        [BurstCompile]
        struct MoveObjectJob : IJobForEachWithEntity<ProjectileSpawnTime, LocalToWorld>
        {
            public EntityCommandBuffer.Concurrent Commands;
            public float TimeSinceLoad;
            public float ProjectileSpeed;
            [ReadOnly] public NativeArray<float> FftArray;

            public void Execute(Entity entity, int index, [ReadOnly] ref ProjectileSpawnTime spawnTime,
                ref LocalToWorld localToWorld)
            {
                var aliveTime = (TimeSinceLoad - spawnTime.SpawnTime);
                if (aliveTime > 5.0f)
                {
                    Commands.DestroyEntity(index, entity);
                }

                var fftIndex = index % 11;
                var translation = new float3(aliveTime * ProjectileSpeed + FftArray[fftIndex] * 11,
                    localToWorld.Position.y,
                    localToWorld.Position.z);
                var rotation = Quaternion.LookRotation(localToWorld.Forward, localToWorld.Up);
                var scale = FftArray[fftIndex];

                localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(
                        translation,
                        rotation,
                        new float3(11f * scale, 1f * scale, 1f * scale))
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
                ProjectileSpeed = 11.0f,
                FftArray = GlobalFftArray
            }.Schedule(this, inputDependencies);
            m_beginSimEcbSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}