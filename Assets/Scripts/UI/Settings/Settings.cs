using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class Settings : MonoBehaviour
{
    public Text AudioText;
    public Text MusicText;
    public Text CameraSensivityText;
    public Text ScreenResolutionText;
    public Text FullscreenText;
    public Text TextSpeedText;

    public List<ScreenResolution> ScreenResolutions;

    private int _audioValue;
    private int _musicValue;
    private int _cameraSensivityValue;
    private int _screenResolutionValue;
    private int _textSpeedValue;
    private bool _isFullscreen;

    private PlayerSettings _playerSettings = new PlayerSettings();

    private ScreenResolution _currentScreenResolution;

    public void Awake()
    {
        SetDefaultValues();

        CheckIfPlayerSettingsExist();

        UpdateCanvasValues();
    }
    private void SetDefaultValues()
    {
        _audioValue = 20;
        _musicValue = 20;
        _cameraSensivityValue = 1;
        _screenResolutionValue = 14;
        _currentScreenResolution = ScreenResolutions[_screenResolutionValue];
        _isFullscreen = true;
        _textSpeedValue = 1;
    }

    private void UpdateCanvasValues()
    {
        AudioText.text = _audioValue + "%";
        MusicText.text = _musicValue + "%";
        CameraSensivityText.text = _cameraSensivityValue.ToString();
        ScreenResolutionText.text = _currentScreenResolution.ScreenWidth + " x " + _currentScreenResolution.ScreenHeight;

        ChangeFullscreenText();

        TextSpeedText.text = ((string)Enum.GetNames(typeof(TextSpeed)).GetValue(_textSpeedValue)).ToLower();
    }

    public void OnChangeAudio(int increment)
    {
        _audioValue = IncrementValues(increment, 100, 0, _audioValue);
        AudioText.text = _audioValue + "%";
    }
    public void OnChangeMusic(int increment)
    {
        _musicValue = IncrementValues(increment, 100, 0, _musicValue);
        MusicText.text = _musicValue + "%";
    }
    public void OnChangeCameraSensitivity(int increment)
    {
        _cameraSensivityValue = IncrementValues(increment, 8, 1, _cameraSensivityValue);
        CameraSensivityText.text = _cameraSensivityValue.ToString();
    }
    public void OnChangeScreenResolution(int increment)
    {
        _screenResolutionValue = IncrementValues(increment, ScreenResolutions.Count - 1, 0, _screenResolutionValue);
        _currentScreenResolution = ScreenResolutions[_screenResolutionValue];
        ScreenResolutionText.text = _currentScreenResolution.ScreenWidth + " x " + _currentScreenResolution.ScreenHeight;
    }

    public void OnChangeFullscreen()
    {
        _isFullscreen = !_isFullscreen;
        ChangeFullscreenText();
    }

    public void OnChangeTextSpeed(int increment)
    {
        _textSpeedValue = IncrementValues(increment, 2, 0, _textSpeedValue);
        TextSpeedText.text = ((string)Enum.GetNames(typeof(TextSpeed)).GetValue(_textSpeedValue)).ToLower();
    }

    private int IncrementValues(int increment, int maxValue, int minValue, int changedValue)
    {
        if (changedValue + increment > maxValue)
        {
            changedValue = minValue;
        }
        else if (changedValue + increment < minValue)
        {
            changedValue = maxValue;
        }
        else
        {
            changedValue += increment;
        }
        return changedValue;
    }

    private void ChangeFullscreenText()
    {
        FullscreenText.text = _isFullscreen ? "yes" : "no";
    }

    // Daryl's save system
    public void OnClickSaveChanges()
    {
        Screen.SetResolution(_currentScreenResolution.ScreenWidth, _currentScreenResolution.ScreenHeight, _isFullscreen);

        _playerSettings.AudioVolume = _audioValue;
        _playerSettings.MusicVolume = _musicValue;
        _playerSettings.CameraSensitivity = _cameraSensivityValue;
        _playerSettings.ScreenResolution = _screenResolutionValue;
        _playerSettings.IsFullscreen = _isFullscreen;
        _playerSettings.TextSpeed = _textSpeedValue;

        ApplySoundChangesInGame();

        _playerSettings.ValidateData();

        SaveHandler.Instance.SaveDataContainer(_playerSettings);
    }

    private void CheckIfPlayerSettingsExist()
    {
        PlayerSettings settings = SaveHandler.Instance.LoadDataContainer<PlayerSettings>();
        if (settings != null)
        {
            OnLoadPlayerSettings(settings);
        }
    }

    private void ApplySoundChangesInGame()
    {
        if (SoundManager.Instance != null)
        {
            // Everything for soundManager
            foreach (Sound sound in SoundManager.Instance.Sounds)
            {
                if (sound.IsMusicVolume)
                    sound.AudioSource.volume = ((float)_musicValue / 100);
                else
                    sound.AudioSource.volume = ((float)_audioValue / 100);
            }

            // Everything not managed by soundmanagers (Entities for example)
            foreach (AudioSource source in FindObjectsOfType<AudioSource>())
            {
                if (!source.GetComponentInParent<SoundManager>())
                    source.volume = ((float)_audioValue / 100);

            }
        }
    }

    private void OnLoadPlayerSettings(PlayerSettings settings)
    {
        _audioValue = settings.AudioVolume;
        _musicValue = settings.MusicVolume;
        _cameraSensivityValue = settings.CameraSensitivity;
        _screenResolutionValue = settings.ScreenResolution;
        _textSpeedValue = settings.TextSpeed;
        _isFullscreen = settings.IsFullscreen;

        _currentScreenResolution = ScreenResolutions[_screenResolutionValue];
        Screen.SetResolution(_currentScreenResolution.ScreenWidth, _currentScreenResolution.ScreenHeight, _isFullscreen);
    }
}