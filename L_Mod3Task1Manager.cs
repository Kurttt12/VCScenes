using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR;  // <-- Needed for XR input and haptics

public class L_Mod3Task1Manager : MonoBehaviour
{
    // Reference to the main Mod3Mod3TaskManagerController33
    public Mod3TaskManagerController3 Mod3TaskManagerController3;

    // UI elements similar to Task2Manager
    public TMP_Text headerText;          // Displays "(INCOMPLETE)", "(COMPLETE)", or "(NEXT TASK)"
    public Transform tasks1Container;    // Container for task toggles (if you have multiple tasks under Task1)
    public GameObject taskTogglePrefab;  // Prefab for each task toggle

    [Header("Gun & Recovery Box")]
    public SimpleShoot gun;              // Reference to the gun's SimpleShoot component
    public Collider recoveryBoxCollider; // The designated collider for the recovery box
    public GameObject bulletUI;          // The UI object that appears when the gun is fired inside the box

    // Bool to indicate if this Task is completed
    public bool taskCompleted = false;

    // A toggle reference if you only have one sub-task here 
    private Toggle taskToggle;

    public TaskTransitionManager3 taskTransitionManager3;

    void Start()
    {
        Debug.Log("Initializing L_Mod3Task1Manager...");

        // Initialize assessment for this task using the updated name "Task1"
        if (AssessmentController.Instance != null)
        {
            AssessmentController.Instance.InitializeTaskAssessment("Task1", 100f);
        }

        // Create a single toggle that describes this sub-task
        taskToggle = CreateTaskToggle("Fire Gun into Recovery Box");

        // Make sure bullet UI is hidden at the start
        if (bulletUI != null)
        {
            bulletUI.SetActive(false);
        }

        UpdateTaskUI();
        UpdateHeader();

        // Subscribe to gunshot event
        if (gun != null)
        {
            gun.OnGunFired += ValidateGunShot;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (gun != null)
        {
            gun.OnGunFired -= ValidateGunShot;
        }
    }

    private Toggle CreateTaskToggle(string taskName)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasks1Container);
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.GetComponentInChildren<TMP_Text>().text = taskName;

        // Typically, you want this toggle non-interactable until the task is active
        toggle.interactable = false;
        return toggle;
    }

    void Update()
    {
        // Ensure UpdateTaskUI is called when this task becomes active
        if (Mod3TaskManagerController3.IsCurrentTask(this.gameObject) && !taskToggle.gameObject.activeSelf)
        {
            Debug.Log("Task1 is now active. Updating Task UI...");
            UpdateTaskUI();
        }
    }

    /// <summary>
    /// Handles toggling UI elements, ensuring the task's toggle is shown only when active.
    /// </summary>
    private void UpdateTaskUI()
    {
        Debug.Log("Updating Mod3Task1 UI...");
        bool isCurrentTask = Mod3TaskManagerController3.IsCurrentTask(this.gameObject);
        Debug.Log($"Is Current Task: {isCurrentTask}");

        if (isCurrentTask)
        {
            // Make the toggle visible and set its state
            taskToggle.gameObject.SetActive(true);
            taskToggle.isOn = taskCompleted;
        }
        else
        {
            // Hide the toggle if this isn't the current task
            taskToggle.gameObject.SetActive(false);
        }

        // If the task is already completed, hide the toggle as well
        if (taskCompleted)
        {
            taskToggle.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when the gun is fired, to check if it's fired inside the recovery box.
    /// If valid, completes the task and shows the bullet UI.
    /// </summary>
    public void ValidateGunShot()
    {
        Debug.Log("Validating gun shot for Task1...");

        // Safety check
        if (gun == null || recoveryBoxCollider == null)
        {
            Debug.LogWarning("Gun or RecoveryBoxCollider not assigned in Task1 manager.");
            return;
        }

        // Check if the gun is within the recovery box bounds
        bool isGunInsideBox = recoveryBoxCollider.bounds.Contains(gun.transform.position);

        if (isGunInsideBox)
        {
            Debug.Log("Gun fired INSIDE the recovery box. Showing bullet UI and marking task complete.");
            // Show bullet UI
            if (bulletUI != null)
            {
                bulletUI.SetActive(true);
            }

            // Log success in the assessment system
            if (AssessmentController.Instance != null)
            {
                AssessmentController.Instance.LogSuccess("Task1", "Gun fired inside recovery box.");
            }

            // Complete the task
            CompleteTask();
        }
        else
        {
            Debug.LogWarning("Gun fired OUTSIDE the recovery box. Task not complete.");
            // Trigger haptic feedback and log failure
            L_Notification.Instance.PlaySound("incorrect");
            L_Notification.Instance.PlaySound("gunshot");
            ShotFailed();
        }
    }

    /// <summary>
    /// Completes the task if it isn't already completed and this is the current task.
    /// </summary>
    public void CompleteTask()
    {
        Debug.Log("Attempting to complete Task1...");

        // Check if not completed yet and is the current task
        if (!taskCompleted && Mod3TaskManagerController3.IsCurrentTask(this.gameObject))
        {
            taskCompleted = true;
            Debug.Log("Task1 completed.");

            // Mark the toggle
            taskToggle.isOn = true;

            // Update UI and headers
            UpdateTaskUI();
            UpdateHeader();

            // Optionally, log success for assessment (if not already logged in ValidateGunShot)
            if (AssessmentController.Instance != null)
            {
                AssessmentController.Instance.LogSuccess("Task1", "Task completed successfully.");
            }

            // Notify the main controller
            Mod3TaskManagerController3.CompleteTask();
            taskTransitionManager3.PostTask1Transition();
        }
        else
        {
            Debug.LogWarning("Task1 is either already complete or not the current active task.");
        }
    }

    /// <summary>
    /// Updates the header text to indicate if the current task is complete or not.
    /// </summary>
    public void UpdateHeader()
    {
        Debug.Log("Updating Task1 manager header...");
        Debug.Log($"Task Completed: {taskCompleted}");
        Debug.Log($"Is Current Task: {Mod3TaskManagerController3.IsCurrentTask(this.gameObject)}");

        if (Mod3TaskManagerController3.IsCurrentTask(this.gameObject))
        {
            headerText.gameObject.SetActive(true);
            headerText.text = "<color=red>(INCOMPLETE)</color> Fire the firearm into the bullet recovery box.";
            Debug.Log("Setting header to incomplete (current task).");
        }
        else
        {
            headerText.gameObject.SetActive(true);
            headerText.text = "<color=green>(COMPLETE)</color> Fire the firearm into the bullet recovery box.";
            Debug.Log("Setting header to complete.");
        }
    }

    /// <summary>
    /// Handles the scenario where the shot is invalid.
    /// This method triggers strong haptic feedback and logs a mistake for assessment.
    /// </summary>
    public void ShotFailed()
    {
        Debug.Log("Shot failed: Gun fired outside the box. Logging deduction for Task1.");

        // Trigger strong haptic feedback on all connected VR devices that support haptics.
        List<InputDevice> devices = new List<InputDevice>();
        // Get all connected devices
        InputDevices.GetDevices(devices);

        foreach (InputDevice device in devices)
        {
            if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
            {
                // Channel 0, amplitude 1.0 (max), duration 0.5 seconds.
                device.SendHapticImpulse(0, 1.0f, 0.5f);
            }
        }

        // Log the mistake in the assessment system with a 10-point deduction.
        if (AssessmentController.Instance != null)
        {
            Debug.Log("Deducting 10 points for firing the gun outside the recovery box.");
            AssessmentController.Instance.LogMistake(
                "Task1",
                "Gun fired outside the recovery box.",
                10f,
                "Ensure the gun is on position with the recovery box before firing."
            );
        }
    }
}
