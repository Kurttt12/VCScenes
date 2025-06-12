using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class TaskManagerController : MonoBehaviour
{
    public Task1Manager task1Manager;
    public Task2Manager task2Manager;
    public Task3Manager task3Manager;
    // Add references to other task managers as needed

    public Canvas completionCanvas; // Reference to the completion canvas

    // Added references to the XR ray interactors for the completion canvas
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;

    private int totalTasks;
    private int completedTasks;
    private int currentTaskIndex = 0;

    // References to the progress bar UI elements
    public Slider progressBar;
    public TMP_Text progressBarText;

    // Define the total number of task managers (here, 3)
    private int totalTaskManagers = 3;

    void Start()
    {
        Debug.Log("Initializing TaskManagerController...");

        // Calculate the total number of tasks.
        totalTasks = task1Manager.taskInfoArray.Length + 1 + task3Manager.evidences.Length; // +1 for Task2Manager

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
        task1Manager.gameObject.SetActive(false);
        task2Manager.gameObject.SetActive(false);
        task3Manager.gameObject.SetActive(false);

        // Activate the current task manager based on the task index.
        if (currentTaskIndex == 0 && task1Manager.CurrentTaskIndex < task1Manager.taskInfoArray.Length)
        {
            task1Manager.gameObject.SetActive(true);
        }
        else if (currentTaskIndex == 1 && !task2Manager.taskCompleted)
        {
            task2Manager.gameObject.SetActive(true);
        }
        else if (currentTaskIndex == 2 && !task3Manager.taskCompleted)
        {
            task3Manager.gameObject.SetActive(true);
        }

        NotifyTaskManagers();
    }

    public bool IsCurrentTask(GameObject taskManager)
    {
        if (currentTaskIndex == 0 && taskManager == task1Manager.gameObject && task1Manager.CurrentTaskIndex < task1Manager.taskInfoArray.Length)
            return true;
        else if (currentTaskIndex == 1 && taskManager == task2Manager.gameObject && !task2Manager.taskCompleted)
            return true;
        else if (currentTaskIndex == 2 && taskManager == task3Manager.gameObject && !task3Manager.taskCompleted)
            return true;
        return false;
    }

    public void CompleteTask()
    {
        // Increment completedTasks if it hasn't exceeded totalTasks.
        if (completedTasks < totalTasks)
        {
            completedTasks++;
            Debug.Log($"Task completed. Total completed: {completedTasks}/{totalTasks}");
            UpdateProgressBar();
            CheckNextTask();

            // Check if all tasks (or skips) are done.
            if (currentTaskIndex >= totalTaskManagers)
            {
                Debug.Log("All tasks have been completed or skipped. Showing completion canvas...");
                ShowCompletionCanvas();
            }
        }
        else
        {
            Debug.Log("All tasks have already been completed. No more tasks to complete.");
            ShowCompletionCanvas();
        }
    }

    private void CheckNextTask()
    {
        if (currentTaskIndex == 0 && task1Manager.CurrentTaskIndex >= task1Manager.taskInfoArray.Length)
        {
            currentTaskIndex++;
        }
        else if (currentTaskIndex == 1 && task2Manager.taskCompleted)
        {
            currentTaskIndex++;
        }
        else if (currentTaskIndex == 2 && task3Manager.taskCompleted)
        {
            currentTaskIndex++;
        }

        UpdateTaskManagers();
    }

    private void NotifyTaskManagers()
    {
        task1Manager.UpdateHeader();
        task2Manager.UpdateHeader();
        task3Manager.UpdateHeader();
        // Notify other task managers as needed.
    }

    // Function to update the progress bar.
    private void UpdateProgressBar()
    {
        float progress = (float)completedTasks / totalTasks * 100;
        if (progress > 100) progress = 100;
        progressBar.value = progress;
        progressBarText.text = $"{progress:F1}%";
    }

    // Function to show the completion canvas, enable ray interactors, position it in front of the player, and pause the game.
public void ShowCompletionCanvas()
{
    if (completionCanvas != null)
    {
        // Add a logger component if one isn't already attached.
        if (completionCanvas.GetComponent<CanvasDisableLogger>() == null)
        {
            completionCanvas.gameObject.AddComponent<CanvasDisableLogger>();
        }

        completionCanvas.gameObject.SetActive(true);
        Debug.Log("Completion canvas activated.");

        // If the canvas has a PauseMenuPositioner, use it to position the canvas.
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
        
        // Pause the game so the canvas remains in view.
        Time.timeScale = 0f;
    }
    else
    {
        Debug.LogError("Completion canvas reference is missing!");
    }
}

// This helper class logs when the canvas is disabled.
public class CanvasDisableLogger : MonoBehaviour
{
    void OnDisable()
    {
        Debug.Log("Completion canvas has been disabled.");
    }
}


    public void SkipTask()
    {
        // Do not increment completedTasks so the progress bar remains unchanged.
        UpdateProgressBar();
        CheckNextTask();
        Debug.Log("Task was skipped. Progress remains unchanged.");

        // Check if all tasks are complete after skipping.
        if (currentTaskIndex >= totalTaskManagers)
        {
            Debug.Log("All tasks have been completed or skipped (after skip). Showing completion canvas...");
            ShowCompletionCanvas();
        }
    }
}