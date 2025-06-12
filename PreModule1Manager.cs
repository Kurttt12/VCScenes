using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PreModule1Manager : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text guideText;
    public TMP_Text descriptionText;
    public TMP_Text[] objectiveTexts; // Array of TMP_Text for objectives
    public TMP_Text[] tutorialTexts; // Array of TMP_Text for tutorials
    public TMP_Text controlsText;
    public Image controlsImage;
    public TMP_Text startText;
    #if UNITY_EDITOR
    public SceneAsset module1Scene; // Scene asset for the next scene
    #endif
    public AudioSource titleSound;
    public AudioSource guideSound;
    public AudioSource descriptionSound;
    public AudioSource objectiveSound;
    public AudioSource tutorialSound;
    public AudioSource controlsSound;
    public AudioSource startSound;
    private float titleDuration = 4f; // Duration to show the title

    private bool proceedRequested = false;

    void Start()
    {
        // Initially hide all UI elements
        titleText.gameObject.SetActive(false);
        guideText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        SetActiveArray(objectiveTexts, false);
        SetActiveArray(tutorialTexts, false);
        controlsText.gameObject.SetActive(false);
        controlsImage.gameObject.SetActive(false);
        startText.gameObject.SetActive(false);

        // Start the sequence
        StartCoroutine(ShowSequence());
    }

    void Update()
    {
        // Check for any key press to proceed
        if (Input.anyKeyDown)
        {
            proceedRequested = true;
        }
    }

    IEnumerator ShowSequence()
    {
        // Show the title text
        titleText.gameObject.SetActive(true);
        if (titleSound != null)
        {
            titleSound.Play();
        }
        yield return new WaitForSeconds(titleDuration);

        // Hide the title and show the guide text
        titleText.gameObject.SetActive(false);
        guideText.gameObject.SetActive(true);
        if (guideSound != null)
        {
            guideSound.Play();
        }

        // Wait for any key press to proceed to the description
        yield return new WaitUntil(() => proceedRequested);
        proceedRequested = false;

        // Hide the guide and show the description text
        guideText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(true);
        if (descriptionSound != null)
        {
            descriptionSound.Play();
        }

        // Wait for any key press to proceed to the objectives
        yield return new WaitUntil(() => proceedRequested);
        proceedRequested = false;

        // Hide the description text and show all objective texts
        descriptionText.gameObject.SetActive(false);
        SetActiveArray(objectiveTexts, true);
        if (objectiveSound != null)
        {
            objectiveSound.Play();
        }

        // Wait for any key press to proceed to the tutorials
        yield return new WaitUntil(() => proceedRequested);
        proceedRequested = false;

        // Hide the objective texts and show all tutorial texts
        SetActiveArray(objectiveTexts, false);
        SetActiveArray(tutorialTexts, true);
        if (tutorialSound != null)
        {
            tutorialSound.Play();
        }

        // Wait for any key press to proceed to the controls
        yield return new WaitUntil(() => proceedRequested);
        proceedRequested = false;

        // Hide the tutorial texts and show the controls text and image
        SetActiveArray(tutorialTexts, false);
        controlsText.gameObject.SetActive(true);
        controlsImage.gameObject.SetActive(true);
        if (controlsSound != null)
        {
            controlsSound.Play();
        }

        // Wait for any key press to proceed to the start screen
        yield return new WaitUntil(() => proceedRequested);
        proceedRequested = false;

        // Hide the controls text and image, and show the start text
        controlsText.gameObject.SetActive(false);
        controlsImage.gameObject.SetActive(false);
        startText.gameObject.SetActive(true);
        if (startSound != null)
        {
            startSound.Play();
        }

        // Wait for any key press to proceed to the main game scene
        yield return new WaitUntil(() => proceedRequested);

        #if UNITY_EDITOR
        // Load the Module1 scene using the scene asset
        SceneManager.LoadScene(module1Scene.name);
        #else
        // Load the Module1 scene by name in build
        SceneManager.LoadScene("Module1");
        #endif
    }

    // Utility method to set active state for all texts in an array
    void SetActiveArray(TMP_Text[] textArray, bool isActive)
    {
        foreach (var text in textArray)
        {
            text.gameObject.SetActive(isActive);
        }
    }
}
