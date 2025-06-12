using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicSlider;
    public Slider sfxSlider;            
    public Slider brightnessSlider;

    [Header("Audio References")]
    public AudioSource musicSource;     // your background‐music AudioSource

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
        musicSlider.onValueChanged.AddListener(v => musicSource.volume = v);

        // —— SFX setup —— 
        // initialize to whatever the first SFX in AudioManager is set to
        if (AudioManager.instance.sounds.Length > 0)
            sfxSlider.value = AudioManager.instance.sounds[0].volume;
        sfxSlider.onValueChanged.AddListener(v => AudioManager.instance.SetGlobalSFXVolume(v));

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

    void SetExposure(float normalizedValue)
    {
        colorAdjustments.postExposure.value =
            Mathf.Lerp(minEV, maxEV, normalizedValue);
    }
}
