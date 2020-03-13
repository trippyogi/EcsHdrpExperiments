using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class OrbFieldSpawnerSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var jobHandle = Entities
            .WithName("SpawnerSystem_EyeLasers")
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex, in OrbFieldSpawner spawner, in LocalToWorld location) =>
            {
                var random = new Random(1);
                var index = 0;

                for (var x = 0; x < spawner.CountX; x++)
                {
                    for (var y = 0; y < spawner.CountY; y++)
                    {
                        var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);
                        var position = math.transform(location.Value,
                            new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 11F) * 2, y * 1.3F));
                        var translation = new Translation { Value = position };
                        
                        commandBuffer.SetComponent(entityInQueryIndex, instance, translation);
                        commandBuffer.SetComponent(entityInQueryIndex, instance, new OrbFieldRotationSpeed
                        {
                            RadiansPerSecond = math.radians(90f)
                        });

                        commandBuffer.SetComponent(entityInQueryIndex, instance,
                            new IndexComponent
                            {
                                Index = index++,
                                InitialPosition = translation
                            });
                    }
                }

                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).Schedule(inputDependencies);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}