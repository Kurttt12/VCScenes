using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class L_Mod3Task2Manager : MonoBehaviour
{
    // Reference to the main Mod3Mod3TaskManagerController33
    public Mod3TaskManagerController3 Mod3TaskManagerController3;

    // UI elements for task status
    public TMP_Text headerText;          // Displays "(INCOMPLETE)" or "(COMPLETE)" when this task is active
    public TMP_Text headerNextText;      // Displays "(NEXT TASK)" when this task is not active
    public Transform tasks2Container;    // Container for task toggles
    public GameObject taskTogglePrefab;  // Prefab for each task toggle

    [Header("Comparison Microscope Setup")]
    // Colliders defining the designated areas for placing the samples
    public Collider bulletFiredCollider;
    public Collider bulletRecoveredCollider;
    
    // Sample objects that must be placed in the designated areas
    public GameObject bulletFiredObject;
    public GameObject bulletRecoveredObject;

    // Task state flags
    public bool bulletFiredPlaced = false;
    public bool bulletRecoveredPlaced = false;
    public bool taskCompleted = false;

    // A toggle reference (if you only have one sub-task here)
    private Toggle taskToggle;

    public TaskTransitionManager3 taskTransitionManager3;

    void Start()
    {
        Debug.Log("Initializing L_Mod3Task2Manager for the Comparison Microscope task...");

        // Create a single toggle that describes this sub-task
        taskToggle = CreateTaskToggle("Place Bullet Fired and Recovered Samples");

        // Initialize header UI
        UpdateTaskUI();
        UpdateHeader();
    }

    private Toggle CreateTaskToggle(string taskName)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasks2Container);
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.GetComponentInChildren<TMP_Text>().text = taskName;
        // Typically, the toggle is non-interactable until the task is active.
        toggle.interactable = false;
        return toggle;
    }

    void Update()
    {
        // Ensure UpdateTaskUI is called when this task becomes active
        if (Mod3TaskManagerController3.IsCurrentTask(this.gameObject) && !taskToggle.gameObject.activeSelf)
        {
            Debug.Log("Mod3 Task 2 is now active. Updating Task UI...");
            UpdateTaskUI();
        }

        // Check if the "Bullet Fired" sample is correctly placed
        if (!bulletFiredPlaced && bulletFiredObject != null && bulletFiredCollider != null)
        {
            if (bulletFiredCollider.bounds.Contains(bulletFiredObject.transform.position))
            {
                bulletFiredPlaced = true;
                Debug.Log("Bullet Fired sample has been placed in the designated area.");
                UpdateHeader();
                CheckTaskCompletion();
            }
        }

        // Check if the "Bullet Recovered" sample is correctly placed
        if (!bulletRecoveredPlaced && bulletRecoveredObject != null && bulletRecoveredCollider != null)
        {
            if (bulletRecoveredCollider.bounds.Contains(bulletRecoveredObject.transform.position))
            {
                bulletRecoveredPlaced = true;
                Debug.Log("Bullet Recovered sample has been placed in the designated area.");
                UpdateHeader();
                CheckTaskCompletion();
            }
        }
    }

    // Checks if both samples have been correctly placed to complete the task.
    private void CheckTaskCompletion()
    {
        if (bulletFiredPlaced && bulletRecoveredPlaced && !taskCompleted)
        {
            taskCompleted = true;
            Debug.Log("Mod3Task2 completed.");
            // Notify the main controller that this task is complete.
            Mod3TaskManagerController3.CompleteTask();
            taskTransitionManager3.PostTask2Transition();
            UpdateHeader();
        }
    }

    /// <summary>
    /// Handles toggling the task UI elements.
    /// </summary>
    private void UpdateTaskUI()
    {
        Debug.Log("Updating Mod3Task2 UI...");
        bool isCurrentTask = Mod3TaskManagerController3.IsCurrentTask(this.gameObject);
        Debug.Log($"Is Current Task: {isCurrentTask}");

        if (isCurrentTask)
        {
            // Show the toggle and update its state.
            taskToggle.gameObject.SetActive(true);
            taskToggle.isOn = taskCompleted;
        }
        else
        {
            // Hide the toggle if this task is not active.
            taskToggle.gameObject.SetActive(false);
        }

        // Hide the toggle if the task is already completed.
        if (taskCompleted)
        {
            taskToggle.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the header text based on the task status:
    /// - Shows red "(INCOMPLETE)" when the task is active but not complete.
    /// - Shows green "(COMPLETE)" when the task is done.
    /// - Shows blue "(NEXT TASK)" when not the current active task.
    /// </summary>
    public void UpdateHeader()
    {
        Debug.Log("Updating L_Mod3Task2Manager header...");
        Debug.Log($"Task Completed: {taskCompleted}");
        Debug.Log($"Is Current Task: {Mod3TaskManagerController3.IsCurrentTask(this.gameObject)}");

        if (Mod3TaskManagerController3.IsCurrentTask(this.gameObject))
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            headerText.text = "<color=red>(INCOMPLETE)</color> Retrieve the bullet and place it into the comparison microscope.";
            Debug.Log("Setting header to incomplete (current task).");
        }
        else if (taskCompleted)
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            headerText.text = "<color=green>(COMPLETE)</color> Retrieve the bullet and place it into the comparison microscope.";
            Debug.Log("Setting header to complete.");
        }
        else
        {
            headerText.gameObject.SetActive(false);
            headerNextText.gameObject.SetActive(true);
            headerNextText.text = "<color=blue>(NEXT TASK)</color> Retrieve the bullet and place it into the comparison microscope.";
            Debug.Log("Setting header to next task.");
        }
    }
}
