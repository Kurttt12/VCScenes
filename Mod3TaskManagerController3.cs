using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class Mod3TaskManagerController3 : MonoBehaviour
{
    // References to the Module 3 task managers
    public L_Mod3Task1Manager L_mod3Task1Manager;
    public L_Mod3Task2Manager L_mod3Task2Manager;
    public L_Mod3Task3Manager L_mod3Task3Manager;

    // Reference to the completion canvas (shown when all tasks are done)
    public Canvas completionCanvas;

    // XR Ray Interactors for the completion canvas (for VR input)
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;

    private int totalTasks;
    private int completedTasks;
    private int currentTaskIndex = 0; // This tracks the active task; 0 = Task1, 1 = Task2, 2 = Task3

    // Progress bar UI elements
    public Slider progressBar;
    public TMP_Text progressBarText;

    void Start()
    {
        Debug.Log("Initializing Mod3TaskManagerController...");

        // Count total tasks (only count tasks that are assigned)
        totalTasks = 0;
        if (L_mod3Task1Manager != null) totalTasks++;
        if (L_mod3Task2Manager != null) totalTasks++;
        if (L_mod3Task3Manager != null) totalTasks++;

        // Hide the completion canvas at startup.
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

    /// <summary>
    /// Enables the current task manager and disables others.
    /// </summary>
    void UpdateTaskManagers()
    {
        // Deactivate all task managers first.
        if (L_mod3Task1Manager != null)
            L_mod3Task1Manager.gameObject.SetActive(false);
        if (L_mod3Task2Manager != null)
            L_mod3Task2Manager.gameObject.SetActive(false);
        if (L_mod3Task3Manager != null)
            L_mod3Task3Manager.gameObject.SetActive(false);

        // Activate the task manager that corresponds to the current task index and is not completed.
        if (currentTaskIndex == 0 && L_mod3Task1Manager != null && !L_mod3Task1Manager.taskCompleted)
        {
            L_mod3Task1Manager.gameObject.SetActive(true);
        }
        else if (currentTaskIndex == 1 && L_mod3Task2Manager != null && !L_mod3Task2Manager.taskCompleted)
        {
            L_mod3Task2Manager.gameObject.SetActive(true);
        }
        else if (currentTaskIndex == 2 && L_mod3Task3Manager != null && !L_mod3Task3Manager.taskCompleted)
        {
            L_mod3Task3Manager.gameObject.SetActive(true);
        }

        NotifyTaskManagers();
    }

    /// <summary>
    /// Returns true if the provided task manager GameObject is the current active task.
    /// </summary>
    public bool IsCurrentTask(GameObject taskManager)
    {
        if (currentTaskIndex == 0 && L_mod3Task1Manager != null &&
            taskManager == L_mod3Task1Manager.gameObject && !L_mod3Task1Manager.taskCompleted)
            return true;
        if (currentTaskIndex == 1 && L_mod3Task2Manager != null &&
            taskManager == L_mod3Task2Manager.gameObject && !L_mod3Task2Manager.taskCompleted)
            return true;
        if (currentTaskIndex == 2 && L_mod3Task3Manager != null &&
            taskManager == L_mod3Task3Manager.gameObject && !L_mod3Task3Manager.taskCompleted)
            return true;
        return false;
    }

    /// <summary>
    /// Increments the count of completed tasks, updates progress,
    /// and checks if all tasks are complete, then advances to the next task.
    /// </summary>
    public void CompleteTask()
    {
        if (completedTasks < totalTasks)
        {
            completedTasks++;
            Debug.Log($"Task completed. Total completed: {completedTasks}/{totalTasks}");
            UpdateProgressBar();
            CheckNextTask();

            // When all tasks are complete (or skipped), show the completion canvas.
            if (currentTaskIndex >= totalTasks)
            {
                Debug.Log("All tasks have been completed or skipped. Showing completion canvas...");
                // ShowCompletionCanvas();
            }
        }
        else
        {
            Debug.Log("All tasks have already been completed.");
            // ShowCompletionCanvas();
        }
    }

    /// <summary>
    /// Checks if the current task is complete and advances to the next task.
    /// </summary>
    private void CheckNextTask()
    {
        // Advance the current task index based on which task is completed.
        if (currentTaskIndex == 0 && L_mod3Task1Manager != null && L_mod3Task1Manager.taskCompleted)
        {
            currentTaskIndex++;
        }
        else if (currentTaskIndex == 1 && L_mod3Task2Manager != null && L_mod3Task2Manager.taskCompleted)
        {
            currentTaskIndex++;
        }
        else if (currentTaskIndex == 2 && L_mod3Task3Manager != null && L_mod3Task3Manager.taskCompleted)
        {
            currentTaskIndex++;
        }

        UpdateTaskManagers();
    }

    /// <summary>
    /// Notifies active task managers to update their headers/UI.
    /// </summary>
    private void NotifyTaskManagers()
    {
        if (L_mod3Task1Manager != null)
            L_mod3Task1Manager.UpdateHeader();
        if (L_mod3Task2Manager != null)
            L_mod3Task2Manager.UpdateHeader();
        if (L_mod3Task3Manager != null)
            L_mod3Task3Manager.UpdateHeader();
    }

    /// <summary>
    /// Updates the progress bar UI to reflect current task completion progress.
    /// </summary>
    private void UpdateProgressBar()
    {
        float progress = (float)completedTasks / totalTasks * 100f;
        if (progress > 100f) progress = 100f;

        if (progressBar != null)
            progressBar.value = progress;
        if (progressBarText != null)
            progressBarText.text = $"{progress:F1}%";
    }

    /// <summary>
    /// Shows the completion canvas, positions it using a PauseMenuPositioner,
    /// enables XR interactors, and pauses the game.
    /// </summary>
    public void ShowCompletionCanvas()
    {
        if (completionCanvas != null)
        {
            // Add a logger component if not already attached.
            if (completionCanvas.GetComponent<CanvasDisableLogger>() == null)
            {
                completionCanvas.gameObject.AddComponent<CanvasDisableLogger>();
            }
            completionCanvas.gameObject.SetActive(true);
            Debug.Log("Completion canvas activated.");

            // Position the canvas if a PauseMenuPositioner component is found.
            PauseMenuPositioner positioner = completionCanvas.GetComponent<PauseMenuPositioner>();
            if (positioner != null)
            {
                positioner.PositionMenu();
            }
            else
            {
                Debug.LogWarning("No PauseMenuPositioner found on the completion canvas.");
            }

            // Enable XR ray interactors.
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

            // Pause the game.
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("Completion canvas reference is missing!");
        }
    }

    /// <summary>
    /// Skips the current task without incrementing the progress bar.
    /// </summary>
    public void SkipTask()
    {
        UpdateProgressBar();
        CheckNextTask();
        Debug.Log("Task was skipped. Progress remains unchanged.");

        if (currentTaskIndex >= totalTasks)
        {
            Debug.Log("All tasks have been completed or skipped (after skip). Showing completion canvas...");
            ShowCompletionCanvas();
        }
    }

    /// <summary>
    /// Helper class to log when the completion canvas is disabled.
    /// </summary>
    public class CanvasDisableLogger : MonoBehaviour
    {
        void OnDisable()
        {
            Debug.Log("Mod3 Completion canvas has been disabled.");
        }
    }
}
