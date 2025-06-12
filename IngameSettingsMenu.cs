using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class IngameSettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicSlider;
    public Slider sfxSlider;            
    public Slider brightnessSlider;

    [Header("Audio References")]
    public AudioSource musicSource;     
    public AudioSource[] sfxSources;    // ⇐ drag your individual SFX AudioSources here

    [Header("URP Volume on Camera")]
    public Volume cameraVolume;

    [Tooltip("Min/Max Exposure in EV (darker to brighter).")]
    public float minEV = -2f;
    public float maxEV = 2f;

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // —— Music setup —— 
        musicSlider.value = musicSource.volume;
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        // —— Sound Effects setup —— 
        if (sfxSources.Length > 0)
            sfxSlider.value = sfxSources[0].volume;  // assume they all start at the same volume
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // —— Brightness setup —— 
        if (!cameraVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.LogError("Color Adjustments override not found on camera volume!");
            return;
        }
        float currentEV = colorAdjustments.postExposure.value;
        brightnessSlider.value = Mathf.InverseLerp(minEV, maxEV, currentEV);
        brightnessSlider.onValueChanged.AddListener(SetExposure);
    }

    /// <summary>
    /// Called whenever the music slider moves.
    /// </summary>
    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
    }

    /// <summary>
    /// Called whenever the SFX slider moves.
    /// Loops through the array and sets each source.
    /// </summary>
    public void SetSFXVolume(float value)
    {
        foreach (var src in sfxSources)
        {
            if (src != null)
                src.volume = value;
        }
    }

    /// <summary>
    /// Called whenever the brightness slider moves.
    /// Maps [0,1] slider to [minEV…maxEV] exposure.
    /// </summary>
    public void SetExposure(float normalizedValue)
    {
        float ev = Mathf.Lerp(minEV, maxEV, normalizedValue);
        colorAdjustments.postExposure.value = ev;
    }
}