using Lasp;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// This system updates all entities in the scene with both a RotationSpeed_SpawnAndRemove and Rotation component.

// ReSharper disable once InconsistentNaming
public class OrbFieldSystem : JobComponentSystem
{
    private const int NumBands = 11;
    readonly float[] fftIn = new float[NumBands];
    readonly float[] fftOut = new float[NumBands];
    readonly float[] amplitude = new float[NumBands];
    private float _fall = 0;
    private float _heightOffset = 0;
    private const float FallDownSpeed = .1f;

    // OnUpdate runs on the main thread.
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

        // var deltaTime = Time.DeltaTime;
        // var time = Time.time;
        //
        // // The in keyword on the RotationSpeed_SpawnAndRemove component tells the job scheduler that this job will not write to rotSpeedSpawnAndRemove
        // return Entities
        //     .WithName("RotationSpeedSystem_SpawnAndRemove")
        //     .ForEach((ref Rotation rotation, in RotationSpeed_EyeLasers rotationSpeedEyeLasers,
        //         in HeightComponent heightComponent) =>
        //     {
        //         var angle = Quaternion.Angle(rotation.Value, Quaternion.LookRotation(Vector3.forward));
        //
        //         // Rotate something about its up vector at the speed given by RotationSpeed_SpawnAndRemove.
        //         rotation.Value = math.mul(math.normalize(rotation.Value),
        //             quaternion.AxisAngle(math.up(),
        //                 rotationSpeedEyeLasers.RadiansPerSecond * deltaTime * math.sin(time) ));
        //     }).Schedule(inputDependencies);
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
                    + index.Index % 11 / 10000f// * math.sin(Time + index.Index) 
                    + FftArray[index.Index % 11] / 3)); // * math.sin(Time)));
            translation.Value.y = index.InitialPosition.Value.y +
                                  math.sin(index.Index + Time / 6 + HeightOffset / 9) * 11; // + FftArray[index.Index % 11]) * 11;
            // translation.Value.y = index.InitialPosition.Value.y + FftArray[index.Index % 11] * 9;// + math.sin(index.Index % 3 * Time);
        }
    }
}