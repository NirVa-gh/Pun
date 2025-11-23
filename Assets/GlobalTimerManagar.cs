using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTimerManagar : MonoBehaviour
{
    [Header("Настройки времени")]
    [SerializeField] private float _baseTimeScale = 1f; // Нормальная скорость
    [SerializeField] private float _minTimeScale = 0.1f; // Минимальное замедление
    [SerializeField] private float _maxTimeScale = 1000f; // Максимальное ускорение

    [Header("Текущее состояние")]
    [SerializeField] private double _universeTime = 0d; // Абсолютное время вселенной в секундах
    [Range(0.1f, 1000f)]
    [SerializeField] private float _currentTimeScale = 1f; // Текущий множитель времени
    [SerializeField] private bool _isPaused = false;

    public event Action<double> OnTimeUpdated; // Передает абсолютное время
    public event Action<float> OnTimeScaleChanged;
    public event Action<bool> OnPauseStateChanged;

    public static GlobalTimerManagar Instance { get; private set; }

    public double UniverseTime => _universeTime;
    public float CurrentTimeScale => _isPaused ? 0f : _currentTimeScale;
    public bool IsPaused => _isPaused;
    public float MinTimeScale => _minTimeScale;
    public float MaxTimeScale => _maxTimeScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_isPaused) return;

        double deltaTime = Time.deltaTime * _currentTimeScale;
        _universeTime += deltaTime;


        OnTimeUpdated?.Invoke(_universeTime);
    }

    public void SetTimeScale(float newTimeScale)
    {
        newTimeScale = Mathf.Clamp(newTimeScale, _minTimeScale, _maxTimeScale);

        if (Mathf.Approximately(_currentTimeScale, newTimeScale)) return;

        _currentTimeScale = newTimeScale;
        OnTimeScaleChanged?.Invoke(_currentTimeScale);
    }

    public void MultiplyTimeScale(float multiplier)
    {
        SetTimeScale(_currentTimeScale * multiplier);
    }

    public void SetPaused(bool paused)
    {
        if (_isPaused == paused) return;

        _isPaused = paused;
        OnPauseStateChanged?.Invoke(_isPaused);
    }
    public void TogglePause()
    {
        SetPaused(!_isPaused);
    }

    public void ResetToNormalSpeed()
    {
        SetTimeScale(_baseTimeScale);
    }

    public void SetNormalSpeed() => SetTimeScale(1f);
    public void SetDoubleSpeed() => SetTimeScale(2f);
    public void SetQuadSpeed() => SetTimeScale(4f);
    public void SetMaxSpeed() => SetTimeScale(_maxTimeScale);
    public string GetFormattedTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(_universeTime);
        var years = (long)_universeTime / 31536000;
        var remainigDays = timeSpan.Days % 365;
        return $"{years}y {remainigDays}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
    }

    public double GetTimeInDays() => _universeTime / 86400; 
    public double GetTimeInYears() => _universeTime / 31536000;
    public double GetTimeInСentury() => _universeTime / 3153600000;

}
