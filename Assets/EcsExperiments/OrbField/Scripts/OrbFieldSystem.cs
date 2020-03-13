using Lasp;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class OrbFieldSystem : JobComponentSystem
{
    private const int NumBands = 11;
    readonly float[] fftIn = new float[NumBands];
    readonly float[] fftOut = new float[NumBands];
    readonly float[] amplitude = new float[NumBands];
    private float _fall = 0;
    private float _heightOffset = 0;
    private const float FallDownSpeed = .1f;

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        MasterInput.RetrieveFft(FftAveragingType.Logarithmic, fftIn, NumBands);
        const float gain = .1f;
        var deltaTime = Time.DeltaTime;
        var time = (float) Time.ElapsedTime;

        for (var i = 0; i < NumBands; i++)
        {
            var input = Mathf.Clamp01(gain * fftIn[i] * (3 * i + 1));
            var dt = deltaTime;
            _fall += Mathf.Pow(10, 1 + FallDownSpeed * 2) * dt;
            fftOut[i] -= _fall * dt;

            if (fftOut[i] < input)
            {
                fftOut[i] = input;
                _fall = 0;
            }

            amplitude[i] = Mathf.Clamp(fftOut[i], 0, 1);
        }

        var result = new NativeArray<float>(fftIn.Length, Allocator.TempJob);
        result.CopyFrom(amplitude);
        _heightOffset += fftOut[7];

        var job = new RotationJob()
        {
            Time = time,
            DeltaTime = deltaTime,
            FftArray = result,
            HeightOffset = _heightOffset
        };

        return job.Schedule(this, inputDependencies);
    }

    [BurstCompile]
    private struct RotationJob : IJobForEach<Rotation, Translation, OrbFieldRotationSpeed,
        IndexComponent>
    {
        [NativeDisableParallelForRestriction] [DeallocateOnJobCompletion]
        public NativeArray<float> FftArray;

        public float Time;
        public float DeltaTime;
        public float HeightOffset;

        public void Execute(ref Rotation rotation, ref Translation translation,
            [ReadOnly] ref OrbFieldRotationSpeed orbFieldSpawner,
            [ReadOnly] ref IndexComponent index)
        {
            var angle = Quaternion.Angle(rotation.Value, Quaternion.LookRotation(Vector3.forward));

            rotation.Value = math.mul(
                math.normalize(rotation.Value),
                quaternion.AxisAngle(math.up(),
                    orbFieldSpawner.RadiansPerSecond * DeltaTime
                    + index.Index % 11 / 10000f
                    + FftArray[index.Index % 11] / 3));
            translation.Value.y = index.InitialPosition.Value.y +
                                  math.sin(index.Index + Time / 6 + HeightOffset / 9) * 11;
        }
    }
}