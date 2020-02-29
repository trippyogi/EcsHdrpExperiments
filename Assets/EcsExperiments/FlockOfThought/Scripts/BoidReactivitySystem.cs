using System.Collections;
using System.Collections.Generic;
using Lasp;
using Samples.Boids;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BoidReactivitySystem : JobComponentSystem
{
    // Fft properties
    private const int NumBands = 11;
    readonly float[] fftIn = new float[NumBands];
    readonly float[] fftOut = new float[NumBands];
    readonly float[] amplitude = new float[NumBands];
    private float _fall = 0;
    private float _heightOffset = 0;
    private const float FallDownSpeed = .3f;
    private NativeArray<float> _result;
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MasterInput.RetrieveFft(FftAveragingType.Logarithmic, fftIn, NumBands);
        const float gain = .1f;
        var deltaTime = math.min(0.05f, Time.DeltaTime);
        var time = (float) Time.ElapsedTime;

        for (var i = 0; i < NumBands; i++)
        {
            var input = Mathf.Clamp01(gain * fftIn[i] * (3 * i + 1));
            var dt = deltaTime;
            // Hold-and-fall-down animation.
            _fall += Mathf.Pow(10, 1 + FallDownSpeed * 2) * dt;
            fftOut[i] -= _fall * dt;

            // Pull up by input.
            if (fftOut[i] < input)
            {
                fftOut[i] = input;
                _fall = 0;
            }

            amplitude[i] = Mathf.Clamp(fftOut[i], 0, 1);
        }

        _result = new NativeArray<float>(fftIn.Length, Allocator.TempJob);
        _result.CopyFrom(amplitude);

        Entities
            .WithName("AudioReactivityJob")
            .WithAll<Boid>()
            .ForEach((Entity entity, int entityInQueryIndex) =>
            {
                // used for audio reactive scale
                var index = entityInQueryIndex % 11;
                var scale = _result[index] * (index + 1);

                // localToWorld.Value = float4x4.TRS(
                //     localToWorld.Value.c3.xyz,
                //     new quaternion(localToWorld.Value),
                //     new float3(scale, scale, scale)
                // );
            }).WithoutBurst().Run();
            //.Schedule(inputDeps);

        return inputDeps;
    }

    protected override void OnDestroyManager()
    {
        base.OnDestroyManager();
        _result.Dispose();
    }
}
