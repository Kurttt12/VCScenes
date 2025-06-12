using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class L_Task2Manager : MonoBehaviour
{
    public L_TaskManagerController L_taskManagerController; // Reference to the L_TaskManagerController
    public TMP_Text headerText; // UI element to display the header for incomplete/complete status
    public TMP_Text headerNextText; // UI element to display the header when the task is the next task
    public Transform tasks2Container; // Container for task toggles for task 2
    public GameObject taskTogglePrefab; // Prefab for task toggles
    public GameObject victimObject; // Reference to the GameObject representing the victim
    public bool taskCompleted = false; // Made this public for progress tracking
    private Toggle taskToggle; // Reference to the task toggle

    public TaskTransitionManager taskTransitionManager;

    void Start()
    {
        Debug.Log("Initializing L_Task2Manager...");
        // Initialize Task2 assessment data.
        AssessmentController.Instance.InitializeTaskAssessment("Task2", 100f);
        
        taskToggle = CreateTaskToggle("Take a photo of the victim");
        UpdateTaskUI();
        UpdateHeader();
    }

    private Toggle CreateTaskToggle(string taskName)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasks2Container);
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.GetComponentInChildren<TMP_Text>().text = taskName;

        toggle.interactable = false;
        return toggle;
    }

    void Update()
    {
        // Ensure UpdateTaskUI is called when Task 2 becomes active
        if (L_taskManagerController.IsCurrentTask(this.gameObject) && !taskToggle.gameObject.activeSelf)
        {
            Debug.Log("Task 2 is now active. Updating Task UI...");
            UpdateTaskUI();
        }
    }

    private void UpdateTaskUI()
    {
        Debug.Log("Updating Task UI...");
        bool isCurrentTask = L_taskManagerController.IsCurrentTask(this.gameObject);
        Debug.Log($"Is current task: {isCurrentTask}");

        if (isCurrentTask)
        {
            Debug.Log("Task is the current active task.");
            taskToggle.gameObject.SetActive(true); // Show the toggle when the task is active
            taskToggle.isOn = taskCompleted; // Ensure the toggle state based on task completion
        }
        else
        {
            Debug.Log("Task is not the current active task.");
            taskToggle.gameObject.SetActive(false); // Hide the toggle when the task is not active
        }

        if (taskCompleted)
        {
            taskToggle.gameObject.SetActive(false); // Hide the toggle when the task is completed
        }
    }

    public void CompleteTask()
    {
        Debug.Log("Attempting to complete task: Photograph the Victim");

        if (!taskCompleted && L_taskManagerController.IsCurrentTask(this.gameObject))
        {
            taskCompleted = true;
            Debug.Log("Task 'Photograph the Victim' completed.");
            taskToggle.isOn = true;
            UpdateTaskUI();
            UpdateHeader();

            taskTransitionManager.PostTask2Transition();
            
            // Log success for Task2 so that it's marked as attempted.
            AssessmentController.Instance.LogSuccess("Task2", "Task 'Photograph the Victim' completed successfully.");
            
            L_taskManagerController.CompleteTask(); // Notify controller that a task is completed
        }
        else
        {
            Debug.LogWarning("Task 'Photograph the Victim' not completed. It may already be completed or it's not the current task.");
        }
    }

    public void OnVictimCaptured()
    {
        if (victimObject != null && !taskCompleted)
        {
            Debug.Log("Victim detected. Marking task as complete.");
            CompleteTask();
        }
        else
        {
            Debug.LogWarning("Victim not detected or task already completed.");
        }
    }

    public void UpdateHeader() // Change this method to public
    {
        Debug.Log("Updating header...");
        Debug.Log($"Task Completed: {taskCompleted}");
        Debug.Log($"Is Current Task: {L_taskManagerController.IsCurrentTask(this.gameObject)}");

        if (L_taskManagerController.IsCurrentTask(this.gameObject))
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            headerText.text = "<color=red>(INCOMPLETE)</color> Photograph the Victim";
            Debug.Log("Setting header to incomplete");
        }
        else if (taskCompleted)
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            headerText.text = "<color=green>(COMPLETE)</color> Photograph the Victim";
            Debug.Log("Setting header to complete");
        }
        else
        {
            headerText.gameObject.SetActive(false);
            headerNextText.gameObject.SetActive(true);
            headerNextText.text = "<color=blue>(NEXT TASK)</color> Photograph the Victim";
            Debug.Log("Setting header to next task");
        }
    }

    /// <summary>
    /// Logs a deduction when the victim (dead body) is not fully in frame during capture.
    /// This method should be called from CameraFunctionality when validation fails.
    /// </summary>
    public void CaptureFailed()
    {
        Debug.Log("Capture failed: Dead body not fully in frame. Logging deduction for Task2.");
        L_Notification.Instance.PlaySound("incorrect");
        L_Notification.Instance.PlaySound("incorrectdeadbody");
        AssessmentController.Instance.LogMistake(
            "Task2",
            "Dead body not fully captured. Ensure the entire victim is in frame before photographing.",
            10f, // Deduction value (adjust as needed)
            "Adjust your camera to fully include the victim in the view."
        );
    }
}
