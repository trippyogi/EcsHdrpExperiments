using System.Collections;
using System.Collections.Generic;
using Lasp;
using Unity.Mathematics;
using UnityEngine;

public class AudioReactiveCamera : MonoBehaviour
{
    // FFT fields
    [SerializeField] FftAveragingType _averagingType = Lasp.FftAveragingType.Logarithmic;
    [SerializeField] [Range(0, 32)] float _inputGain = 1.0f;
    [SerializeField] [Range(1, 64)] int _fftBands = 11;
    [SerializeField] bool _holdAndFallDown = true;
    [SerializeField, Range(0, 1)] float _fallDownSpeed = 0.1f;

    private int _bands;
    private float _fall = 0;
    private float[] _fftIn, _fftOut;

    // Camera fields
    [SerializeField] [Range(0, 100)] float _xRange = 100;
    [SerializeField] [Range(0, 10)] float _yRange = 10;
    [SerializeField] [Range(0, 100)] float _zRange = 100;
    [SerializeField] [Range(0, 100)] float _bgSpeed = 10;
    [SerializeField] [Range(0, 10)] float _cameraSpeed = .3f;

    private int _xDirection, _yDirection, _zDirection, _rotationDirection, _bgColorIndex;
    private Camera _cam;
    private Color[] _bgColors;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        UpdateFft();
        UpdateCameraOrientation();
        UpdateCameraBackgroundColor();
    }

    void Initialize()
    {
        _bands = _fftBands;
        _fftIn = new float[_fftBands];
        _fftOut = new float[_fftBands];

        _xDirection = 1;
        _yDirection = 1;
        _zDirection = 1;
        _rotationDirection = 1;

        _cam = GetComponent<Camera>();
        _bgColors = new[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan, 
            Color.magenta, 
            Color.yellow,
        };
        _bgColorIndex = 0;
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

    void UpdateCameraOrientation()
    {
        if (_bands != _fftBands)
            Initialize();

        if (gameObject.transform.position.x > _xRange || gameObject.transform.position.x < 10)
            _xDirection *= -1;
        if (gameObject.transform.position.y > _yRange || gameObject.transform.position.y < -_yRange)
            _yDirection *= -1;
        if (gameObject.transform.position.z > _zRange || gameObject.transform.position.z < 10)
            _zDirection *= -1;
        if (_fftOut[4] > .9f)
            _rotationDirection *= -1;

        var position = gameObject.transform.position;
        position +=
            new Vector3(Mathf.Clamp(_fftOut[1], 0, 1) * _xDirection * _cameraSpeed,
                Mathf.Clamp(_fftOut[6], 0, 1) * _yDirection * _cameraSpeed,
                Mathf.Clamp(_fftOut[8], 0, 1) * _zDirection * 3 * _cameraSpeed);
        gameObject.transform.position = position;
        gameObject.transform.RotateAround(position, transform.up, Time.deltaTime + _fftOut[5] * _rotationDirection + _fftOut[9] * 5);
    }

    void UpdateCameraBackgroundColor()
    {
        if (_fftOut[10] > .01f)
            _bgColorIndex = (_bgColorIndex + 1) % _bgColors.Length;

        _cam.backgroundColor = _bgColors[_bgColorIndex];
        // _cam.backgroundColor = Color.HSVToRGB(Mathf.PingPong(Time.time * _bgSpeed / 100 + _fftOut[6], 1), 1, 1);
    }
}