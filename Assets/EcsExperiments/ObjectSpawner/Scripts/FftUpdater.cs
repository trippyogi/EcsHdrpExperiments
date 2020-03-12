using System;
using System.Collections;
using System.Collections.Generic;
using Advanced.ObjectSpawner;
using Lasp;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

// Updates and manages the FftInput to be used globally for all audio reactive systems
public class FftUpdater : MonoBehaviour
{
    [SerializeField] FftAveragingType _averagingType = Lasp.FftAveragingType.Logarithmic;
    [SerializeField] [Range(0, 32)] float _inputGain = 1.0f;
    [SerializeField] [Range(1, 64)] int _fftBands = 11;
    [SerializeField] bool _holdAndFallDown = true;
    [SerializeField, Range(0, 1)] float _fallDownSpeed = 0.1f;

    private float _fall = 0;
    private float[] _fftIn, _fftOut;

    private MoveObjectSystem _spawnerSystem;

    void Start()
    {
        Initialize();
        _spawnerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MoveObjectSystem>();
        _spawnerSystem.GlobalFftArray = new NativeArray<float>(_fftBands, Allocator.TempJob);
    }

    void Update()
    {
        UpdateFft();
        if (_spawnerSystem == null)
            _spawnerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MoveObjectSystem>();
        _spawnerSystem.GlobalFftArray.CopyFrom(_fftOut);
        // _spawnerSystem.Update();
    }

    private void OnDestroy()
    {
        _spawnerSystem.GlobalFftArray.Dispose();
    }

    void Initialize()
    {
        _fftIn = new float[_fftBands];
        _fftOut = new float[_fftBands];
    }

    void UpdateFft()
    {
        Lasp.MasterInput.RetrieveFft(_averagingType, _fftIn, _fftBands);
        var gain = _averagingType == FftAveragingType.Linear ? _inputGain : _inputGain / 10.0f;
        for (var i = 0; i < _fftBands; i++)
        {
            var input = Mathf.Clamp01(gain * _fftIn[i] * (3 * i + 1));
            var dt = Time.deltaTime;
            if (_holdAndFallDown)
            {
                // Hold-and-fall-down animation.
                _fall += Mathf.Pow(10, 1 + _fallDownSpeed * 2) * dt;
                _fftOut[i] -= _fall * dt;

                // Pull up by input.
                if (_fftOut[i] < input)
                {
                    _fftOut[i] = input;
                    _fall = 0;
                }
            }
            else
            {
                _fftOut[i] = input;
            }
        }
    }
}