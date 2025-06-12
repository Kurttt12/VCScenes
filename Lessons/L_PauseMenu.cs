using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class L_PauseMenu : MonoBehaviour
{
    public InputActionAsset inputActions;
    public Canvas L_pauseMenuCanvas;    // Reference to the pause menu canvas
    public Canvas settingsCanvas;     // Reference to the settings menu canvas
    public Button resumeButton;       // Reference to the resume button
    public Button restartButton;      // Reference to the restart button\
    public Button endAssessmentButton; // Reference to the end assessment button
    public Button settingsButton;     // Reference to the settings button
    public Button quitButton;         // Reference to the quit button
    public Button backButton;         // Reference to the Settings Back button

    public L_PauseMenuPositioner menuPositioner;          // Positions the menu in front of the player's view
    public XRRayInteractor leftRayInteractor;           // Left-hand XR Ray Interactor
    public XRRayInteractor rightRayInteractor;          // Right-hand XR Ray Interactor

    // Teleportation specific objects:
    public TeleportationProvider teleportationProvider;  // Handles teleportation requests
    public Transform designatedRoomTransform;            // Target transform of the designated room

    private InputAction _menuButtonAction;

    private void Start()
    {
        // Start with the pause and settings menus hidden and the ray interactors disabled.
        L_pauseMenuCanvas.enabled = false;
        settingsCanvas.enabled = false;
        leftRayInteractor.gameObject.SetActive(false);
        rightRayInteractor.gameObject.SetActive(false);

        // Set up the Menu action from the input system.
        _menuButtonAction = inputActions.FindActionMap("XRI LeftHand").FindAction("Menu");
        _menuButtonAction.Enable();
        _menuButtonAction.performed += ToggleL_PauseMenu;

        // Add listeners to the existing buttons.
        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(RestartGame);
        settingsButton.onClick.AddListener(ToggleSettings);
        quitButton.onClick.AddListener(Quit);
        backButton.onClick.AddListener(BackToL_PauseMenu);

        // Set up the new end assessment button.
        if (endAssessmentButton != null)
        {
            endAssessmentButton.onClick.AddListener(EndAssessment);
        }
    }

    private void OnDestroy()
    {
        if (_menuButtonAction != null)
        {
            _menuButtonAction.performed -= ToggleL_PauseMenu;
        }
    }

    public void ToggleL_PauseMenu(InputAction.CallbackContext context)
    {
        // Toggle the pause menu visibility.
        bool isL_PauseMenuVisible = !L_pauseMenuCanvas.enabled;
        L_pauseMenuCanvas.enabled = isL_PauseMenuVisible;
        settingsCanvas.enabled = false; // Ensure the settings menu is hidden.

        if (isL_PauseMenuVisible)
        {
            // Position the pause menu and enable the ray interactors.
            menuPositioner.PositionMenu();
            leftRayInteractor.gameObject.SetActive(true);
            rightRayInteractor.gameObject.SetActive(true);
        }
        else
        {
            // Disable the ray interactors.
            leftRayInteractor.gameObject.SetActive(false);
            rightRayInteractor.gameObject.SetActive(false);
        }

        // Pause or resume the game by adjusting the time scale.
        Time.timeScale = isL_PauseMenuVisible ? 0f : 1f;
    }

    public void ToggleSettings()
    {
        // Toggle the settings canvas visibility and hide the pause menu when active.
        settingsCanvas.enabled = !settingsCanvas.enabled;
        L_pauseMenuCanvas.enabled = !settingsCanvas.enabled;
    }

    public void Resume()
    {
        L_pauseMenuCanvas.enabled = false;
        settingsCanvas.enabled = false;
        leftRayInteractor.gameObject.SetActive(false);
        rightRayInteractor.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        // Restart the current active scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        Debug.Log("Going back to main menu.");
        Resume();
    }

    /// <summary>
    /// Called when the Settings Back button is clicked.
    /// Re-enables the pause menu while hiding the settings.
    /// </summary>
    public void BackToL_PauseMenu()
    {
        settingsCanvas.enabled = false;
        L_pauseMenuCanvas.enabled = true;
        leftRayInteractor.gameObject.SetActive(true);
        rightRayInteractor.gameObject.SetActive(true);
    }

    /// <summary>
    /// Ends the assessment by finalizing assessments, triggering teleportation, 
    /// updating the assessment UI, and then resuming the game.
    /// </summary>
    public void EndAssessment()
    {
        Debug.Log("End assessment button pressed from L_PauseMenu.");

        // Teleport the player to the designated room.
        if (teleportationProvider != null && designatedRoomTransform != null)
        {
            TeleportRequest teleportRequest = new TeleportRequest()
            {
                destinationPosition = designatedRoomTransform.position,
                destinationRotation = designatedRoomTransform.rotation
            };
            teleportationProvider.QueueTeleportRequest(teleportRequest);
        }
        else
        {
            Debug.LogWarning("TeleportationProvider or designatedRoomTransform is not set.");
        }

        // Finalize assessments using the existing AssessmentController.
        if (AssessmentController.Instance != null)
        {
            AssessmentController.Instance.FinalizeAssessments();
            string overallReport = AssessmentController.Instance.GenerateOverallReport();
            Debug.Log("Overall Report: " + overallReport);

            // Update the Assessment UI.
            AssessmentReportUI reportUI = FindObjectOfType<AssessmentReportUI>();
            if (reportUI != null)
            {
                reportUI.DisplayReport(overallReport);
                reportUI.DisplayIndividualReports();
            }
            else
            {
                Debug.LogWarning("No AssessmentReportUI found in the scene.");
            }
        }
        else
        {
            Debug.LogWarning("AssessmentController instance not found!");
        }

        // Resume the game and hide the pause menu.
        Time.timeScale = 1f;
        L_pauseMenuCanvas.enabled = false;
        settingsCanvas.enabled = false;
        leftRayInteractor.gameObject.SetActive(false);
        rightRayInteractor.gameObject.SetActive(false);
    }
}
