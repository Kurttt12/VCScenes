using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement; // For scene management

public class L_PauseTask : MonoBehaviour
{
    public InputActionAsset inputActions;
    public Canvas L_PauseTaskCanvas;         // Reference to the pause menu canvas
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
        L_PauseTaskCanvas.enabled = false;
        leftRayInteractor.gameObject.SetActive(false);
        rightRayInteractor.gameObject.SetActive(false);

        // Get the Menu action for the pause menu from the Righthand
        _menuButtonAction = inputActions.FindActionMap("XRI Righthand").FindAction("Task Configuration");
        _menuButtonAction.Enable();
        _menuButtonAction.performed += ToggleL_PauseTask;

        // Add listeners to buttons
        resumeButton.onClick.AddListener(Resume);
        nexttaskButton.onClick.AddListener(NextTask);
        nextmoduleButton.onClick.AddListener(NextModule);
    }

    private void OnDestroy()
    {
        if (_menuButtonAction != null)
        {
            _menuButtonAction.performed -= ToggleL_PauseTask;
        }
    }

    public void ToggleL_PauseTask(InputAction.CallbackContext context)
    {
        bool isL_PauseTaskVisible = !L_PauseTaskCanvas.enabled;
        L_PauseTaskCanvas.enabled = isL_PauseTaskVisible;

        if (isL_PauseTaskVisible)
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

        Time.timeScale = isL_PauseTaskVisible ? 0f : 1f;
    }

    public void Resume()
    {
        L_PauseTaskCanvas.enabled = false;
        leftRayInteractor.gameObject.SetActive(false);
        rightRayInteractor.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void NextTask()
    {
        Debug.Log("Next Task button pressed...");
        L_TaskManagerController controller = FindObjectOfType<L_TaskManagerController>();

        // Determine which task manager is active and call its skip method.
        if (controller.IsCurrentTask(controller.L_task1Manager.gameObject))
        {
            controller.L_task1Manager.SkipCurrentTask();
        }
        else if (controller.IsCurrentTask(controller.L_task2Manager.gameObject))
        {
            Debug.Log("Skipping Task 2: Capture the Victim");
        }
        else if (controller.IsCurrentTask(controller.L_task3Manager.gameObject))
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
