
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class HUDGlobalTimerManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _timeDisplay;
    [SerializeField] private TextMeshProUGUI _speedDisplay;
    [SerializeField] private Slider _timeScaleSlider;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _speed1xButton;
    [SerializeField] private Button _speed2xButton;
    [SerializeField] private Button _speed4xButton;
    [SerializeField] private Button _maxSpeedButton;

    private void OnEnable()
    {

    }

    private void Start()
    {
        GlobalTimerManagar.Instance.OnTimeUpdated += UpdateTimeDisplay;
        GlobalTimerManagar.Instance.OnTimeScaleChanged += UpdateSpeedDisplay;
        GlobalTimerManagar.Instance.OnPauseStateChanged += UpdatePauseState;

        _pauseButton.onClick.AddListener(() => GlobalTimerManagar.Instance.SetPaused(true));
        _playButton.onClick.AddListener(() => GlobalTimerManagar.Instance.SetPaused(false));
        _speed1xButton.onClick.AddListener(GlobalTimerManagar.Instance.SetNormalSpeed);
        _speed2xButton.onClick.AddListener(GlobalTimerManagar.Instance.SetDoubleSpeed);
        _speed4xButton.onClick.AddListener(GlobalTimerManagar.Instance.SetQuadSpeed);
        _maxSpeedButton.onClick.AddListener(GlobalTimerManagar.Instance.SetMaxSpeed);

        _timeScaleSlider.minValue = GlobalTimerManagar.Instance.MinTimeScale;
        _timeScaleSlider.maxValue = GlobalTimerManagar.Instance.MaxTimeScale;
        _timeScaleSlider.onValueChanged.AddListener(OnSliderValueChanged);

        UpdateTimeDisplay(GlobalTimerManagar.Instance.UniverseTime);
        UpdateSpeedDisplay(GlobalTimerManagar.Instance.CurrentTimeScale);
    }

    private void UpdatePauseState(bool isPaused)
    {
        _pauseButton.gameObject.SetActive(!isPaused);
        _playButton.gameObject.SetActive(isPaused);
    }

    private void UpdateSpeedDisplay(float timeScale)
    {
        _speedDisplay.text = $"{timeScale:F1}x";
        _timeScaleSlider.SetValueWithoutNotify(timeScale);
    }

    private void UpdateTimeDisplay(double universeTime)
    {
        _timeDisplay.text = GlobalTimerManagar.Instance.GetFormattedTime();
    }
    private void OnSliderValueChanged(float value)
    {
        GlobalTimerManagar.Instance.SetTimeScale(value);
    }
    private void OnDisable()
    {
        if (GlobalTimerManagar.Instance != null)
        {
            GlobalTimerManagar.Instance.OnTimeUpdated -= UpdateTimeDisplay;
            GlobalTimerManagar.Instance.OnTimeScaleChanged -= UpdateSpeedDisplay;
            GlobalTimerManagar.Instance.OnPauseStateChanged -= UpdatePauseState;
        }
    }

}
