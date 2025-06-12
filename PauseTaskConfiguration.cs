using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement; // For scene management

public class PauseTask : MonoBehaviour
{
    public InputActionAsset inputActions;
    public Canvas PauseTaskCanvas;         // Reference to the pause menu canvas
    public Button resumeButton;            // Reference to the resume button
    public Button nexttaskButton;          // Reference to the Next Task button
    public Button nextmoduleButton;        // Reference to the Next Module button
    public PauseMenuPositioner menuPositioner; // Reference to the menu positioner script
    public XRRayInteractor leftRayInteractor;  // Reference to the left hand XR Ray Interactor
    public XRRayInteractor rightRayInteractor; // Reference to the right hand XR Ray Interactor

    private InputAction _menuButtonAction;

    private void Start()
    {
        // Start with the pause menu hidden and ray interactors disabled
        PauseTaskCanvas.enabled = false;
        leftRayInteractor.gameObject.SetActive(false);
        rightRayInteractor.gameObject.SetActive(false);

        // Get the Menu action for the pause menu from the Righthand
        _menuButtonAction = inputActions.FindActionMap("XRI Righthand").FindAction("Task Configuration");
        _menuButtonAction.Enable();
        _menuButtonAction.performed += TogglePauseTask;

        // Add listeners to buttons
        resumeButton.onClick.AddListener(Resume);
        nexttaskButton.onClick.AddListener(NextTask);
        nextmoduleButton.onClick.AddListener(NextModule);
    }

    private void OnDestroy()
    {
        if (_menuButtonAction != null)
        {
            _menuButtonAction.performed -= TogglePauseTask;
        }
    }

    public void TogglePauseTask(InputAction.CallbackContext context)
    {
        bool isPauseTaskVisible = !PauseTaskCanvas.enabled;
        PauseTaskCanvas.enabled = isPauseTaskVisible;

        if (isPauseTaskVisible)
        {
            menuPositioner.PositionMenu();
            leftRayInteractor.gameObject.SetActive(true);
            rightRayInteractor.gameObject.SetActive(true);
        }
        else
        {
            leftRayInteractor.gameObject.SetActive(false);
            rightRayInteractor.gameObject.SetActive(false);
        }

        Time.timeScale = isPauseTaskVisible ? 0f : 1f;
    }

    public void Resume()
    {
        PauseTaskCanvas.enabled = false;
        leftRayInteractor.gameObject.SetActive(false);
        rightRayInteractor.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void NextTask()
    {
        Debug.Log("Next Task button pressed...");
        TaskManagerController controller = FindObjectOfType<TaskManagerController>();

        // Determine which task manager is active and call its skip method.
        if (controller.IsCurrentTask(controller.task1Manager.gameObject))
        {
            controller.task1Manager.SkipCurrentTask();
        }
        else if (controller.IsCurrentTask(controller.task2Manager.gameObject))
        {
            Debug.Log("Skipping Task 2: Capture the Victim");
        }
        else if (controller.IsCurrentTask(controller.task3Manager.gameObject))
        {
            Debug.Log("Skipping Task 3: Secure the Victim");
            
        }

        // Resume the game after skipping.
        Resume();
    }

    public void NextModule()
    {
        Debug.Log("Next Module button pressed...");
        // Replace "Pre-Module 2" with your actual next scene/module name.
        SceneManager.LoadScene("Pre-Module 2");
    }
}
