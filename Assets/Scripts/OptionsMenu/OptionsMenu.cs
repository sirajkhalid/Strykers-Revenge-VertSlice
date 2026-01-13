using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown graphicsDropdown;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;  

    private readonly Vector2Int[] fixedResolutions =
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 720),
        new Vector2Int(960, 540)
    };

    void Start()
    {
        LoadResolution();
        LoadGraphicsQuality();
        LoadVolume();
    }

    // ---------------------------------------------------------
    // RESOLUTION
    // ---------------------------------------------------------
    void LoadResolution()
    {
        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);

        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        // apply on startup
        SetResolution(savedIndex);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= fixedResolutions.Length) return;

        Vector2Int res = fixedResolutions[index];
        Screen.SetResolution(res.x, res.y, FullScreenMode.Windowed);

        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    // ---------------------------------------------------------
    // GRAPHICS QUALITY
    // ---------------------------------------------------------
    void LoadGraphicsQuality()
    {
        int savedIndex = PlayerPrefs.GetInt("GraphicsIndex", 0);

        graphicsDropdown.value = savedIndex;
        graphicsDropdown.RefreshShownValue();

        graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);

        // apply on startup
        SetGraphicsQuality(savedIndex);
    }

    public void SetGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("GraphicsIndex", index);
    }

    // ---------------------------------------------------------
    // VOLUME (MUSIC / SFX)
    // ---------------------------------------------------------
    void LoadVolume()
    {
        float music = PlayerPrefs.GetFloat("MusicVol", 1);
        float sfx = PlayerPrefs.GetFloat("SFXVol", 1);

        musicSlider.value = music;
        sfxSlider.value = sfx;

        SetMusicVolume(music);
        SetSFXVolume(sfx);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVol", value);

        // Converts linear slider value → decibels for mixer
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVol", value);

        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
    }
}
