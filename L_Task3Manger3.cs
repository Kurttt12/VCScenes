using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR; // Added for haptic feedback
using System.Collections.Generic;

public class L_LiftingManager : MonoBehaviour
{
    public L_TaskManagerController2 taskManagerController;

    [Header("UI Elements")]
    public TMP_Text headerText;        // Displays current status (COMPLETE/INCOMPLETE)
    public TMP_Text headerNextText;    // Displays next-step guidance (e.g., "Proceed to next module")

    public Transform tasksContainer;   // Container for lifting task toggles
    public GameObject taskTogglePrefab; // Prefab for UI toggles

    [Header("Tape Task Objects")]
    public GameObject tape;            // The tape asset with the fingerprint (hidden by default)
    public GameObject doorFingerprint; // The fingerprint on the door (visible initially)

    [Header("Card Task Objects")]
    public GameObject cardAsset;           // The card asset with a fingerprint (hidden by default)
    public float cardTransferDistance = 0.5f; // Distance threshold for transferring fingerprint to card

    [Header("Distance Thresholds")]
    public float tapeApplyDistance = 0.3f; // Distance threshold to apply tape (Task 1)
    public float tapeLiftDistance = 0.5f;  // Distance threshold to lift tape (Task 2)

    private bool tapeApplied = false;  
    private bool tapeLifted = false;   
    private bool cardRevealed = false; 

    // New variables for slow pull detection
    private Vector3 lastTapePosition;
    // The maximum speed (units/second) allowed to consider the pull as "slow"
    private float slowPullThreshold = 0.2f;
    // Flag to indicate a fast pull has been detected (to prevent spamming logs)
    private bool fastPullDetected = false;

    public TaskTransitionManager2 taskTransitionManager2;

    void Start()
    {
        // Ensure the tape asset is hidden at the start.
        if (tape != null)
        {
            tape.SetActive(false);
            lastTapePosition = tape.transform.position; // Initialize tape position
        }

        // Door fingerprint remains active initially.
        if (doorFingerprint != null)
            doorFingerprint.SetActive(true);

        // Card asset should be hidden initially.
        if (cardAsset != null)
            cardAsset.SetActive(false);

        // Create UI toggles for all three subtasks.
        CreateTaskToggle("Apply Tape on Fingerprint", false);
        CreateTaskToggle("Lift the Tape", false);
        CreateTaskToggle("Transfer Fingerprint to Card", false);

        UpdateHeader();
    }

    void Update()
    {
        // Task 1: Apply Tape on Fingerprint
        if (!tapeApplied && tape != null && doorFingerprint != null)
        {
            float distanceToDoor = Vector3.Distance(tape.transform.position, doorFingerprint.transform.position);
            if (distanceToDoor <= tapeApplyDistance)
            {
                ApplyTape();
            }
        }

        // Task 2: Lift the Tape
        if (tapeApplied && !tapeLifted && tape != null && doorFingerprint != null)
        {
            float distanceToDoor = Vector3.Distance(tape.transform.position, doorFingerprint.transform.position);
            // Calculate the tape's pulling speed
            float tapeSpeed = (tape.transform.position - lastTapePosition).magnitude / Time.deltaTime;
            Debug.Log("Tape pulling speed: " + tapeSpeed.ToString("F2") + " units/sec.");

            if (distanceToDoor >= tapeLiftDistance)
            {
                if (!fastPullDetected)
                {
                    if (tapeSpeed <= slowPullThreshold)
                    {
                        LiftTape();
                    }
                    else if (tapeSpeed > slowPullThreshold)
                    {
                        Debug.Log("Tape pulled too fast. Please pull slowly.");
                        // Log the mistake using our helper function.
                        L_Notification.Instance.PlaySound("incorrect");
                        L_Notification.Instance.PlaySound("lift_too_fast");
                        LogTaskMistake(
                            "Tape pulled too fast.",
                            10f,
                            "Pull the tape slowly to ensure proper transfer."
                        );
                        // Trigger strong haptic feedback when pulled too fast.
                        TriggerHaptics(1.0f, 0.2f);
                        fastPullDetected = true;
                    }
                }
            }
            else
            {
                // Reset the fast pull flag when the tape collides with the door fingerprint again.
                fastPullDetected = false;
            }
        }

        // Task 3: Transfer Fingerprint to Card
        if (tapeLifted && !cardRevealed && tape != null && cardAsset != null)
        {
            float distanceToCard = Vector3.Distance(tape.transform.position, cardAsset.transform.position);
            if (distanceToCard <= cardTransferDistance)
            {
                RevealCardFingerprint();
            }
        }

        // Update lastTapePosition for the next frame.
        if (tape != null)
            lastTapePosition = tape.transform.position;
    }

    /// <summary>
    /// Helper method for logging a mistake for Task3.
    /// </summary>
    private void LogTaskMistake(string description, float deduction, string tip)
    {
        AssessmentController.Instance.LogMistake("Task3", description, deduction, tip);
    }

    private void CreateTaskToggle(string taskName, bool isActive)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasksContainer);
        TMP_Text toggleText = toggleObject.GetComponentInChildren<TMP_Text>();
        toggleText.text = $"{taskName} ({(isActive ? "Active" : "Inactive")})";

        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.interactable = false;
        toggle.isOn = isActive;
    }

    private void UpdateToggle(string taskName, bool isActive)
    {
        foreach (Transform child in tasksContainer)
        {
            TMP_Text toggleText = child.GetComponentInChildren<TMP_Text>();
            if (toggleText.text.Contains(taskName))
            {
                toggleText.text = $"{taskName} ({(isActive ? "Active" : "Inactive")})";
                child.GetComponent<Toggle>().isOn = isActive;
                break;
            }
        }
    }

    public void UpdateHeader()
    {
        if (headerText != null)
        {
            if (tapeApplied && tapeLifted && cardRevealed)
            {
                headerText.text = "<color=green>(COMPLETE)</color> Lifting task completed.";
            }
            else
            {
                headerText.text = "<color=red>(INCOMPLETE)</color> Complete the lifting task.";
            }
        }

        if (headerNextText != null)
        {
            if (tapeApplied && tapeLifted && cardRevealed)
            {
                headerNextText.text = "All tasks in Module 2 complete. Proceed to the next module!";
            }
            else
            {
                headerNextText.text = "";
            }
        }
    }

    private void ApplyTape()
    {
        tapeApplied = true;
        UpdateToggle("Apply Tape on Fingerprint", true);
        Debug.Log("Tape applied on door fingerprint.");
        // Log success for Task1
        AssessmentController.Instance.LogSuccess("Task1", "Tape applied on door fingerprint successfully.");
        UpdateHeader();
        CheckCompletion();
    }

    private void LiftTape()
    {
        tapeLifted = true;

        // Hide the door fingerprint and reveal the tape asset.
        if (doorFingerprint != null)
            doorFingerprint.SetActive(false);
        if (tape != null)
            tape.SetActive(true);

        UpdateToggle("Lift the Tape", true);
        Debug.Log("Tape lifted. Tape asset is now visible.");
        // Log success for Task2
        AssessmentController.Instance.LogSuccess("Task2", "Tape lifted successfully.");
        UpdateHeader();
        CheckCompletion();
    }

    private void RevealCardFingerprint()
    {
        cardRevealed = true;
        if (cardAsset != null)
            cardAsset.SetActive(true);

        UpdateToggle("Transfer Fingerprint to Card", true);
        Debug.Log("Card fingerprint is now revealed.");
        // Log success for Task3
        AssessmentController.Instance.LogSuccess("Task3", "Fingerprint transferred to card successfully.");
        taskTransitionManager2.PostTask3Transition();
        UpdateHeader();
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (tapeApplied && tapeLifted && cardRevealed)
        {
            taskManagerController.CompleteTask();
        }
    }

    // Haptic feedback method to trigger on both controllers.
    private void TriggerHaptics(float amplitude, float duration)
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDeviceCharacteristics controllerCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.HeldInHand;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        foreach (var device in devices)
        {
            if (device.TryGetHapticCapabilities(out UnityEngine.XR.HapticCapabilities capabilities) && capabilities.supportsImpulse)
            {
                uint channel = 0; // most devices use channel 0.
                device.SendHapticImpulse(channel, amplitude, duration);
            }
        }
    }

    public int GetTotalTasks()
    {
        return 1;
    }

    public bool TaskCompleted
    {
        get { return tapeApplied && tapeLifted && cardRevealed; }
    }
}
