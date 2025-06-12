using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class L_TaskManagerController2 : MonoBehaviour
{
    // References to task managers.
    public L_FingerprintManager L_fingerprintManager;
    public L_PowderManager L_powderManager;
    public L_LiftingManager L_liftingManager;

    public Canvas completionCanvas; // Reference to the completion canvas

    // XR ray interactors for the completion canvas.
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;

    private int totalTasks;
    private int completedTasks;
    private int currentTaskIndex = 0;

    // Progress bar UI elements.
    public Slider progressBar;
    public TMP_Text progressBarText;

    // Updated to reflect three task managers in Module 2.
    private int totalTaskManagers = 3; 

    void Start()
    {
        Debug.Log("Initializing L_TaskManagerController2 for Personal Identification Module...");

        // Sum total tasks from all managers.
        totalTasks = L_fingerprintManager.GetTotalTasks() + L_powderManager.GetTotalTasks() + L_liftingManager.GetTotalTasks();

        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Completion canvas is not assigned!");
        }

        UpdateTaskManagers();
        UpdateProgressBar();
    }

    void UpdateTaskManagers()
    {
        // Deactivate all task managers initially.
        L_fingerprintManager.gameObject.SetActive(false);
        L_powderManager.gameObject.SetActive(false);
        L_liftingManager.gameObject.SetActive(false);

        // Activate the current task manager based on the task index.
        if (currentTaskIndex == 0 && !L_fingerprintManager.TaskCompleted)
        {
            L_fingerprintManager.gameObject.SetActive(true);
        }
        else if (currentTaskIndex == 1 && !L_powderManager.TaskCompleted)
        {
            L_fingerprintManager.gameObject.SetActive(true);
            L_powderManager.gameObject.SetActive(true);
        }
        else if (currentTaskIndex == 2 && !L_liftingManager.TaskCompleted)
        {
            L_liftingManager.gameObject.SetActive(true);
        }

        NotifyTaskManagers();
    }

    public bool IsCurrentTask(GameObject taskManager)
    {
        if (currentTaskIndex == 0 && taskManager == L_fingerprintManager.gameObject && !L_fingerprintManager.TaskCompleted)
            return true;
        if (currentTaskIndex == 1 && taskManager == L_powderManager.gameObject && !L_powderManager.TaskCompleted)
            return true;
        if (currentTaskIndex == 2 && taskManager == L_liftingManager.gameObject && !L_liftingManager.TaskCompleted)
            return true;

        return false;
    }

    public void CompleteTask()
    {
        if (completedTasks < totalTasks)
        {
            completedTasks++;
            Debug.Log($"Task completed. Total completed: {completedTasks}/{totalTasks}");
            UpdateProgressBar();
            CheckNextTask();

            if (currentTaskIndex >= totalTaskManagers)
            {
                Debug.Log("All tasks have been completed or skipped. Showing completion canvas...");
                // ShowCompletionCanvas();
            }
        }
    }

    private void CheckNextTask()
    {
        if (currentTaskIndex == 0 && L_fingerprintManager.TaskCompleted)
        {
            currentTaskIndex++;
        }
        else if (currentTaskIndex == 1 && L_powderManager.TaskCompleted)
        {
            currentTaskIndex++;
        }
        else if (currentTaskIndex == 2 && L_liftingManager.TaskCompleted)
        {
            currentTaskIndex++;
        }
        UpdateTaskManagers();
    }

    private void NotifyTaskManagers()
    {
        L_fingerprintManager.UpdateHeader();
        L_powderManager.UpdateHeader();
        L_liftingManager.UpdateHeader();
    }

    // Update the progress bar UI.
    private void UpdateProgressBar()
    {
        float progress = (float)completedTasks / totalTasks * 100f;
        if (progress > 100f)
            progress = 100f;
        progressBar.value = progress;
        progressBarText.text = $"{progress:F1}%";
    }

    // Show the completion canvas, enable ray interactors, and pause the game.
    public void ShowCompletionCanvas()
    {
        if (completionCanvas != null)
        {
            completionCanvas.gameObject.SetActive(true);

            PauseMenuPositioner positioner = completionCanvas.GetComponent<PauseMenuPositioner>();
            if (positioner != null)
            {
                positioner.PositionMenu();
            }
            else
            {
                Debug.LogWarning("No PauseMenuPositioner found on the completion canvas.");
            }

            if (leftRayInteractor != null)
            {
                leftRayInteractor.gameObject.SetActive(true);
                Debug.Log("Left Ray Interactor enabled.");
            }
            if (rightRayInteractor != null)
            {
                rightRayInteractor.gameObject.SetActive(true);
                Debug.Log("Right Ray Interactor enabled.");
            }

            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("Completion canvas reference is missing!");
        }
    }

    public void SkipTask()
    {
        UpdateProgressBar();
        CheckNextTask();
        Debug.Log("Task was skipped. Progress remains unchanged.");

        if (currentTaskIndex >= totalTaskManagers)
        {
            Debug.Log("All tasks have been completed or skipped (after skip). Showing completion canvas...");
            // ShowCompletionCanvas();
        }
    }
}