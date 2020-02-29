using System.Collections;
using System.Collections.Generic;
using Lasp;
using Unity.Mathematics;
using UnityEngine;

public class CameraPump : MonoBehaviour
{
    // FFT fields
    [SerializeField] FftAveragingType _averagingType = Lasp.FftAveragingType.Logarithmic;
    [SerializeField] [Range(0, 32)] float _inputGain = 1.0f;
    [SerializeField] [Range(1, 64)] int _fftBands = 11;
    [SerializeField] bool _holdAndFallDown = true;
    [SerializeField, Range(0, 1)] float _fallDownSpeed = 0.1f;
    
    [SerializeField] float _baseDistance = 300;
    [SerializeField] [Range(0,10)] int _pumpIndex = 3;
    [SerializeField] [Range(1,10000)] float _pumpScale = 300;

    private int _bands;
    private float _fall = 0;
    private float[] _fftIn, _fftOut;

    // Camera fields
    private Camera _cam;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        UpdateFft();
        UpdateCamera();
    }

    void Initialize()
    {
        _bands = _fftBands;
        _fftIn = new float[_fftBands];
        _fftOut = new float[_fftBands];

        _cam = GetComponent<Camera>();
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

    void UpdateCamera()
    {
        if (_bands != _fftBands)
            Initialize();

        var position = new Vector3(0, 0, _baseDistance - Mathf.Clamp(_fftOut[_pumpIndex], 0, 1) * _pumpScale);
        _cam.transform.localPosition = position;
    }
}