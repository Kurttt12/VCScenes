using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SubtitleManager : MonoBehaviour
{
    [System.Serializable]
    public class SubtitlePanel
    {
        [Tooltip("Optional: Set this only if you want to override the existing text")]
        public string overrideText;
        public float delayBeforeShow;
        public float displayDuration;
        public GameObject panelObject;
        [Tooltip("Optional: Reference to the Text or TextMeshProUGUI component (only needed if using overrideText)")]
        public Component textComponent;
    }

    [Header("Activation Method")]
    [Tooltip("Choose how the subtitles should be activated")]
    public ActivationMethod activationMethod = ActivationMethod.Both;
    
    public enum ActivationMethod
    {
        ButtonOnly,
        ColliderOnly,
        Both
    }

    [Header("Settings")]
    [Tooltip("Link this to your audio trigger script")]
    public AudioSource linkedAudioSource;
    
    [Tooltip("The Canvas that contains all subtitle panels")]
    public Canvas subtitleCanvas;
    
    [Header("Button Configuration")]
    [Tooltip("The button that will trigger the subtitle display")]
    public Button triggerButton;

    [Header("Collider Configuration")]
    [Tooltip("Use this GameObject's collider or specify another")]
    public bool useThisCollider = true;
    [Tooltip("Reference to a specific collider if not using this GameObject's")]
    public Collider triggerCollider;
    [Tooltip("Layer mask for objects that can trigger the collider")]
    public LayerMask triggerLayers = ~0; // Default to "Everything"
    [Tooltip("Tag of objects that can trigger the collider (leave empty for any)")]
    public string triggerTag = "";
    
    [Header("Subtitle Configuration")]
    [Tooltip("List of subtitle panels with their timing information")]
    public List<SubtitlePanel> subtitlePanels = new List<SubtitlePanel>();

    private bool isShowingSubtitles = false;
    private bool hasActivated = false;     // ← new flag
    private Coroutine subtitleCoroutine;

    public void Awake()
    {
        // Set up the button click listener if a button is assigned and should be used
        if (triggerButton != null && 
            (activationMethod == ActivationMethod.ButtonOnly || 
             activationMethod == ActivationMethod.Both))
        {
            triggerButton.onClick.AddListener(ShowSubtitles);
        }
        
        // If using this object's collider
        if (useThisCollider && 
            (activationMethod == ActivationMethod.ColliderOnly || 
             activationMethod == ActivationMethod.Both))
        {
            // Add box collider if it doesn't exist
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
                GetComponent<BoxCollider>().isTrigger = true;
            }
            else if (GetComponent<Collider>() is BoxCollider)
            {
                // Ensure it's a trigger
                GetComponent<BoxCollider>().isTrigger = true;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // If we’ve already triggered once, do nothing
        if (hasActivated) 
            return;

        // Only proceed if collider-activation is enabled
        if (activationMethod == ActivationMethod.ColliderOnly ||
            activationMethod == ActivationMethod.Both)
        {
            // Layer mask check
            if (((1 << other.gameObject.layer) & triggerLayers) == 0)
                return;

            // Tag check (if specified)
            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag))
                return;

            // Mark as used so future entries are ignored
            hasActivated = true;

            // Show subtitles and (optionally) play audio
            ShowSubtitles();
            if (linkedAudioSource != null && !linkedAudioSource.isPlaying)
                linkedAudioSource.Play();
        }
    }

    // Call this method from your button press script to start showing subtitles
    public void ShowSubtitles()
    {
        if (isShowingSubtitles)
        {
            StopCoroutine(subtitleCoroutine);
        }

        subtitleCoroutine = StartCoroutine(ShowSubtitlesSequence());
    }

    // This will automatically be called if you've linked the audio source
    public void OnEnable()
    {
        if (linkedAudioSource != null)
        {
            // Add listener to audio start event if you have one
            // Or you can call ShowSubtitles() from the same button that plays the audio
        }
    }

    private IEnumerator ShowSubtitlesSequence()
    {
        isShowingSubtitles = true;
        
        // Make sure the canvas is active
        if (subtitleCanvas != null)
        {
            subtitleCanvas.gameObject.SetActive(true);
        }
        
        // Hide all panels initially
        foreach (SubtitlePanel panel in subtitlePanels)
        {
            if (panel.panelObject != null)
            {
                panel.panelObject.SetActive(false);
            }
        }
        
        // Show each panel at its specified time
        foreach (SubtitlePanel panel in subtitlePanels)
        {
            // Wait for the delay before showing this panel
            yield return new WaitForSeconds(panel.delayBeforeShow);
            
            if (panel.panelObject != null)
            {
                // Show the panel
                panel.panelObject.SetActive(true);
                
                // Only update text if overrideText is provided and a text component is assigned
                if (!string.IsNullOrEmpty(panel.overrideText) && panel.textComponent != null)
                {
                    if (panel.textComponent is Text)
                    {
                        (panel.textComponent as Text).text = panel.overrideText;
                    }
                    else if (panel.textComponent is TextMeshProUGUI)
                    {
                        (panel.textComponent as TextMeshProUGUI).text = panel.overrideText;
                    }
                }
                
                // Wait for the display duration
                yield return new WaitForSeconds(panel.displayDuration);
                
                // Hide the panel after its duration
                panel.panelObject.SetActive(false);
            }
        }
        
        // Optionally hide the canvas when all subtitles are finished
        if (subtitleCanvas != null)
        {
            subtitleCanvas.gameObject.SetActive(false);
        }
        
        isShowingSubtitles = false;
    }

    // For debugging/testing
    public void StartSubtitlesWithAudio()
    {
        if (linkedAudioSource != null)
        {
            linkedAudioSource.Play();
            ShowSubtitles();
        }
    }
    
    // Stop subtitles if needed
    public void StopSubtitles()
    {
        if (isShowingSubtitles && subtitleCoroutine != null)
        {
            StopCoroutine(subtitleCoroutine);
            
            // Hide all panels
            foreach (SubtitlePanel panel in subtitlePanels)
            {
                if (panel.panelObject != null)
                {
                    panel.panelObject.SetActive(false);
                }
            }
            
            // Hide the canvas
            if (subtitleCanvas != null)
            {
                subtitleCanvas.gameObject.SetActive(false);
            }
            
            isShowingSubtitles = false;
        }
    }
}