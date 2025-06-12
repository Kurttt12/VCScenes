using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Task1Manager : MonoBehaviour
{
    public TaskManagerController taskManagerController; // Reference to the TaskManagerController
    public TMP_Text headerText; // UI element to display the header
    public Transform tasksContainer; // Container for task toggles
    public GameObject taskTogglePrefab; // Prefab for task toggles
    private int currentTaskIndex = 0;
    private List<TaskInfo> tasks = new List<TaskInfo>();
    public int CurrentTaskIndex { get { return currentTaskIndex; } }

    [System.Serializable]
    public struct TaskInfo
    {
        public string taskName;
        public TaskTrigger taskTrigger;
        public Toggle taskToggle; // Reference to the toggle
        public List<CylinderTrigger> cylinderTriggers; // List of CylinderTriggers for each task
        public bool wasSkipped; // Flag to record if this task was skipped
    }
    public TaskInfo[] taskInfoArray;

    void Start()
    {
        // Build the tasks list from the inspector array.
        foreach (var taskInfo in taskInfoArray)
        {
            TaskInfo newTask = taskInfo;
            newTask.wasSkipped = false; // Initialize flag
            newTask.taskToggle = CreateTaskToggle(taskInfo.taskName);
            tasks.Add(newTask);
        }

        UpdateTaskUI();
        UpdateHeader();

        // Initialize assessment for Task1 with a max score of 100.
        AssessmentController.Instance.InitializeTaskAssessment("Task1", 100f);

        ActivateNextTask();
    }

    private Toggle CreateTaskToggle(string taskName)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasksContainer);
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        int totalCount = taskInfoArray[currentTaskIndex].cylinderTriggers.Count;
        toggle.GetComponentInChildren<TMP_Text>().text = $"{taskName} (0/{totalCount})";
        toggle.interactable = false;
        return toggle;
    }

    private void UpdateTaskUI()
    {
        if (currentTaskIndex >= tasks.Count)
        {
            foreach (Transform child in tasksContainer)
                Destroy(child.gameObject);
            return;
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            // If the task was skipped, mark toggle off; otherwise, mark it as complete if its index is less than current.
            tasks[i].taskToggle.isOn = tasks[i].wasSkipped ? false : (i < currentTaskIndex);
            
            int triggeredCount = taskInfoArray[i].cylinderTriggers.Count(t => t.isTriggered);
            int totalCount = taskInfoArray[i].cylinderTriggers.Count;
            string statusSuffix = tasks[i].wasSkipped ? " (Skipped)" : $" ({triggeredCount}/{totalCount})";
            tasks[i].taskToggle.GetComponentInChildren<TMP_Text>().text = tasks[i].taskName + statusSuffix;
        }

        if (currentTaskIndex < tasks.Count)
            Debug.Log($"Current task: {tasks[currentTaskIndex].taskName}");
        else
            Debug.Log("All tasks completed or processed!");

        UpdateCylinderUIVisibility();
    }

    public bool CompleteTask(string taskName)
    {
        Debug.Log($"Attempting to complete task: {taskName}");
        Debug.Log($"Current task index: {currentTaskIndex}, Current task: {tasks[currentTaskIndex].taskName}");

        if (currentTaskIndex < tasks.Count && tasks[currentTaskIndex].taskName == taskName)
        {
            // Mark the current task as complete
            tasks[currentTaskIndex].taskToggle.isOn = true; 
            currentTaskIndex++;

            // Call global progress update only once per task completion.
            taskManagerController.CompleteTask();
            Debug.Log($"Task '{taskName}' completed.");

            // Log success via your assessment system.
            AssessmentController.Instance.LogSuccess("Task1", $"Task '{taskName}' completed successfully.");

            UpdateTaskUI();
            UpdateHeader();

            // Instead of calling taskManagerController.CompleteTask() here again,
            // simply log that all subtasks are done.
            if (currentTaskIndex >= tasks.Count)
            {
                Debug.Log("All tasks have been completed.");
            }
            else
            {
                Debug.Log($"Moving to the next task: {tasks[currentTaskIndex].taskName}");
            }
            return true;
        }
        else
        {
            Debug.LogWarning($"Task '{taskName}' not completed. Either already completed or not the current task.");
            return false;
        }
    }


    public void UpdateHeader()
    {
        if (currentTaskIndex < tasks.Count)
        {
            headerText.text = "<color=red>(INCOMPLETE)</color> Take a picture of:";
        }
        else
        {
            bool anySkipped = tasks.Any(t => t.wasSkipped);
            headerText.text = anySkipped ? "<color=red>(INCOMPLETE)</color> Capturing the Scene" 
                                          : "<color=green>(COMPLETE)</color> Capturing the Scene";
        }
    }

    public void TriggerEntered(TaskTrigger trigger)
    {
        Debug.Log($"TaskTrigger entered: {trigger.gameObject.name}");
        if (trigger.isTriggered)
        {
            Debug.Log($"TaskTrigger {trigger.gameObject.name} is marked as triggered.");
            UpdateTaskUI();
            CheckAllTriggersInTask(currentTaskIndex);
        }
    }

    public void TriggerEntered(CylinderTrigger trigger)
    {
        Debug.Log($"CylinderTrigger entered: {trigger.gameObject.name}");
        if (trigger.isTriggered)
        {
            Debug.Log($"CylinderTrigger {trigger.gameObject.name} is marked as triggered.");
            UpdateTaskUI();
            CheckAllTriggersInTask(currentTaskIndex);
        }
    }

    private void CheckAllTriggersInTask(int taskIndex)
    {
        bool allTriggersActivated = true;
        // Check all cylinder triggers for the current task.
        foreach (var cylinderTrigger in taskInfoArray[taskIndex].cylinderTriggers)
        {
            if (!cylinderTrigger.isTriggered)
            {
                allTriggersActivated = false;
                break;
            }
        }
        // Also check the main task trigger.
        if (!taskInfoArray[taskIndex].taskTrigger.isTriggered)
            allTriggersActivated = false;

        if (allTriggersActivated)
        {
            Debug.Log($"All triggers in task {taskIndex + 1} have been activated.");
            CompleteTask(tasks[taskIndex].taskName);
            ActivateNextTask();
        }
    }

    private void ActivateNextTask()
    {
        if (currentTaskIndex < tasks.Count)
            Debug.Log($"Activating next task: {tasks[currentTaskIndex].taskName}");
    }

    private void UpdateCylinderUIVisibility()
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            bool showUI = (i == currentTaskIndex);
            foreach (var cylinderTrigger in tasks[i].cylinderTriggers)
            {
                if (cylinderTrigger != null && cylinderTrigger.associatedMeshRenderer != null)
                {
                    if (!cylinderTrigger.isTriggered)
                        cylinderTrigger.associatedMeshRenderer.enabled = showUI;
                }
            }
        }
    }

    public void SkipCurrentTask()
    {
        if (currentTaskIndex < tasks.Count)
        {
            TaskInfo currentTask = tasks[currentTaskIndex];
            string taskName = currentTask.taskName;

            foreach (var trigger in currentTask.cylinderTriggers)
            {
                if (trigger != null && trigger.gameObject != null)
                    trigger.gameObject.SetActive(false);
            }

            currentTask.wasSkipped = true;
            currentTask.taskToggle.GetComponentInChildren<TMP_Text>().text = taskName + " (Skipped)";
            tasks[currentTaskIndex] = currentTask;

            Debug.Log($"Task '{taskName}' skipped.");
            SoundNotification.Instance.PlaySound("incorrect");
            AssessmentController.Instance.LogMistake("Task1", "Task was skipped.", 10f, "Avoid skipping tasks if possible.");

            currentTaskIndex++;
            taskManagerController.SkipTask();

            UpdateTaskUI();
            UpdateHeader();

            if (currentTaskIndex >= tasks.Count)
            {
                Debug.Log("All tasks complete for module 1(after skip).");
            }
        }
        else
        {
            Debug.LogWarning("No current task to skip.");
        }
    }

/// <summary>
/// Finalizes all Task1 subtasks.
/// Iterates over the tasks list and logs a deduction if a task is not fully completed.
/// For each task:
/// - If neither the main task trigger nor any cylinder trigger is activated, it is considered "not attempted."
/// - If the task was attempted (i.e. at least one trigger activated) but not all cylinder triggers are activated, it is considered "partially completed."
/// The base deduction per task is calculated as 100 divided by the total number of tasks.
/// </summary>
public void FinalizeTaskSubtasks()
{
    Debug.Log("Finalizing Task Subtasks...");
    List<string> notAttemptedTasks = new List<string>();
    List<string> partiallyCompletedTasks = new List<string>();

    for (int i = 0; i < tasks.Count; i++)
    {
        Debug.Log($"Evaluating task {i}: {tasks[i].taskName}");

        // Skip tasks that were explicitly skipped.
        if (tasks[i].wasSkipped)
        {
            Debug.Log($"Task '{tasks[i].taskName}' was skipped. No deduction.");
            continue;
        }

        bool mainTriggered = tasks[i].taskTrigger != null && tasks[i].taskTrigger.isTriggered;
        bool anyCylinderTriggered = tasks[i].cylinderTriggers != null && tasks[i].cylinderTriggers.Any(c => c.isTriggered);
        Debug.Log($"Task '{tasks[i].taskName}': mainTriggered = {mainTriggered}, anyCylinderTriggered = {anyCylinderTriggered}");

        if (!mainTriggered && !anyCylinderTriggered)
        {
            notAttemptedTasks.Add(tasks[i].taskName);
        }
        else
        {
            bool allCylindersActivated = tasks[i].cylinderTriggers != null && tasks[i].cylinderTriggers.All(c => c.isTriggered);
            if (!allCylindersActivated)
            {
                partiallyCompletedTasks.Add(tasks[i].taskName);
            }
            else
            {
                Debug.Log($"Task '{tasks[i].taskName}' appears complete. No deduction.");
            }
        }
    }

    // Calculate the base deduction per task.
    float baseDeduction = 100f / tasks.Count; 

    if (notAttemptedTasks.Count > 0)
    {
        string tasksList = string.Join(", ", notAttemptedTasks);
        float totalDeduction = notAttemptedTasks.Count * baseDeduction;
        Debug.Log($"Tasks not attempted: {tasksList} (Total Deduction: {totalDeduction})");
        AssessmentController.Instance.LogMistake(
            "Task1",
            $"Task '{tasksList}' was not attempted.",
            totalDeduction,
            "Complete the capture for these tasks."
        );
    }

    if (partiallyCompletedTasks.Count > 0)
    {
        string tasksList = string.Join(", ", partiallyCompletedTasks);
        float totalDeduction = partiallyCompletedTasks.Count * baseDeduction;
        Debug.Log($"Tasks partially completed: {tasksList} (Total Deduction: {totalDeduction})");
        AssessmentController.Instance.LogMistake(
            "Task1",
            $"Task '{tasksList}' was partially completed.",
            totalDeduction,
            "Complete capturing the designated areas for these tasks."
        );
    }

    Debug.Log("All tasks have been processed. Finalizing Task1 assessment.");
}
}
