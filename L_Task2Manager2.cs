using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR; // For haptic feedback on XR devices
using System.Collections.Generic;

public class L_PowderManager : MonoBehaviour
{
    public L_TaskManagerController2 taskManagerController;

    [Header("UI Elements")]
    public TMP_Text headerText;        // Displays current status (COMPLETE/INCOMPLETE)
    public TMP_Text headerNextText;    // Displays next-step guidance (like "Proceed to Lifting Task")
    
    public Transform tasksContainer;   // Container for powder task toggles
    public GameObject taskTogglePrefab; // Prefab for UI toggles

    [Header("Powder Task Objects")]
    public GameObject brush;             // The brush asset with powder (initially inactive)
    public GameObject powderContainer;   // The container holding the powder
    public float powderApplyDistance = 0.5f; // Distance threshold for applying powder

    [Header("Fingerprint Reveal Objects")]
    public GameObject fingerprintObject; // Fingerprint to be revealed after brushing
    public Material fingerprintMaterial; // Material for controlling fingerprint transparency
    public float fingerprintRevealDistance = 0.5f; // Distance threshold for brushing the fingerprint

    private bool brushPowdered = false;
    private bool fingerprintBrushed = false;

    // // Variables for powder interaction timing.
    // private bool isInPowderInteraction = false;
    // private float interactionStartTime = 0f;
    // private bool lightHapticTriggered = false;
    // private bool strongHapticTriggered = false;

    // --- Variables for tap detection using vertical movement ---
    private bool hasShaken = false;           // Set when a valid tap is detected.
    private float lastTapY = 0f;              // Tracks the last vertical (y) position of the brush.
    // Modified: increased tap vertical threshold from 0.01 to 0.02 units.
    private float tapVerticalThreshold = 0.02f; 
    private float tapCooldown = 1f;           // Cooldown period after a tap is detected.
    private float tapTimer = 0f;              // Timer for tap cooldown.
    private float promptInterval = 1f;        // Interval before showing the prompt.
    private float promptTimer = 0f;           // Timer for prompting.
    private bool promptShown = false;         // Ensures the prompt isn't spammed.
    private bool hasStartedTapDetection = false; // Flag to log tap detection start once.
    // --- End tap detection variables ---

    // New variable to record when the brush has left the powder container.
    private float brushOutsideTime = 0f;

    public TaskTransitionManager2 taskTransitionManager2;

    void Start()
    {
        // Initially hide the brush and fingerprint.
        if (brush != null)
        {
            brush.SetActive(false);
            // Initialize lastTapY with the brush's starting y position.
            lastTapY = brush.transform.position.y;
        }

        if (fingerprintObject != null)
        {
            fingerprintObject.SetActive(false);
            SetFingerprintVisibility(false);
        }

        // Create UI toggles dynamically for each subtask.
        CreateTaskToggle("Apply Powder on Brush", false);
        CreateTaskToggle("Brush the Fingerprint", false);

        UpdateHeader();
    }

void Update()
{
    // --- Instant Powder Application ---
    if (!brushPowdered && brush != null && powderContainer != null)
    {
        float dist = Vector3.Distance(brush.transform.position, powderContainer.transform.position);
        if (dist <= powderApplyDistance)
        {
            ApplyPowder();
        }
    }

    // --- Tap Detection Process (unchanged) ---
    if (brushPowdered && !fingerprintBrushed && brush != null && (Time.time - brushOutsideTime >= 1f))
    {
        if (!hasStartedTapDetection)
        {
            lastTapY = brush.transform.position.y;
            Debug.Log("Starting vertical displacement measurement now (1 second after leaving powder container).");
            hasStartedTapDetection = true;
        }
        DetectTap();
    }

    // --- Fingerprint Reveal Section (unchanged) ---
    if (brushPowdered && !fingerprintBrushed && brush != null && fingerprintObject != null)
    {
        float distanceToFingerprint = Vector3.Distance(brush.transform.position, fingerprintObject.transform.position);
        if (distanceToFingerprint <= fingerprintRevealDistance && hasShaken)
        {
            BrushFingerprint();
        }
        else if (distanceToFingerprint > fingerprintRevealDistance)
        {
            promptShown = false;
            promptTimer = 0f;
        }
    }
}


    // Detects a discrete vertical tap based on the brush's y-axis movement.
    private void DetectTap()
    {
        float currentY = brush.transform.position.y;
        float yDifference = Mathf.Abs(currentY - lastTapY);
        Debug.Log("Vertical displacement: " + yDifference.ToString("F2") + " units.");

        // If not in cooldown and the vertical change exceeds the new threshold (0.02 units), register a tap.
        if (tapTimer <= 0f && yDifference > tapVerticalThreshold)
        {
            // If displacement is 0.03 or more, trigger strong haptic feedback, log a mistake, and reset the powder task.
            if (yDifference >= 0.03f)
            {
                Debug.Log("Excessive tap detected (" + yDifference.ToString("F2") + " units). Resetting powder task.");
                L_Notification.Instance.PlaySound("incorrect");
                L_Notification.Instance.PlaySound("excess_shaking");
                AssessmentController.Instance.LogMistake(
                    "Task2",
                    "Excessive tapping detected.",
                    10f,
                    "Tap gently with minimal vertical movement."
                );
                TriggerHaptics(1.0f, 0.2f); // Strong haptic feedback.
                ResetPowderProcess();
            }
            else
            {
                hasShaken = true;
                Debug.Log("Vertical tap detected with displacement: " + yDifference.ToString("F2") + " units.");
                // Trigger haptic feedback if displacement is at least 0.02 units (and less than 0.03).
                if (yDifference >= 0.02f)
                {
                    TriggerHaptics(0.3f, 0.1f);
                }
            }
            tapTimer = tapCooldown;
            promptTimer = 0f;
            promptShown = false;
        }
        else
        {
            tapTimer -= Time.deltaTime;
            promptTimer += Time.deltaTime;
            if (!promptShown && promptTimer >= promptInterval)
            {
                Debug.Log("Please tap (move the brush up and down) the controller.");
                TriggerHaptics(0.5f, 0.1f);
                promptShown = true;
            }
        }
        lastTapY = currentY;
    }

    // Resets the powder task so the user must reapply the powder.
    private void ResetPowderProcess()
    {
        brushPowdered = false;
        fingerprintBrushed = false;
        hasShaken = false;
        hasStartedTapDetection = false;
        // Optionally, hide the brush again.
        if (brush != null)
            brush.SetActive(false);
        // Update UI toggles to inactive.
        UpdateToggle("Apply Powder on Brush", false);
        UpdateToggle("Brush the Fingerprint", false);
        UpdateHeader();
        Debug.Log("Powder task reset. Please apply powder again.");
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
            if (brushPowdered && fingerprintBrushed)
            {
                headerText.text = "<color=green>(COMPLETE)</color> Powder task completed.";
            }
            else
            {
                headerText.text = "<color=red>(INCOMPLETE)</color> Complete the powder task.";
            }
        }
        if (headerNextText != null)
        {
            if (brushPowdered && fingerprintBrushed)
            {
                headerNextText.text = "Proceed to the Lifting Task.";
            }
            else
            {
                headerNextText.text = "";
            }
        }
    }

    private void ApplyPowder()
    {
        brushPowdered = true;
        if (brush != null)
            brush.SetActive(true);

        UpdateToggle("Apply Powder on Brush", true);
        Debug.Log("Powder applied to brush.");
        // Record the time when the brush is confirmed outside the powder container.
        brushOutsideTime = Time.time;
        UpdateHeader();
        CheckCompletion();
    }

private void BrushFingerprint()
{
    if (fingerprintBrushed)
        return;

    fingerprintBrushed = true;
    if (fingerprintObject != null)
    {
        fingerprintObject.SetActive(true);
        SetFingerprintVisibility(true);
    }
    UpdateToggle("Brush the Fingerprint", true);
    Debug.Log("Fingerprint brushed with powdered brush; fingerprint is now permanently visible.");

    // Ensure the task is logged as successfully attempted.
    AssessmentController.Instance.LogSuccess("Task2", "Fingerprint brushing completed successfully.");
    taskTransitionManager2.PostTask2Transition();
    
    UpdateHeader();
    CheckCompletion();
}


    private void CheckCompletion()
    {
        if (brushPowdered && fingerprintBrushed)
        {
            taskManagerController.CompleteTask();
        }
    }

    private void SetFingerprintVisibility(bool isVisible)
    {
        if (fingerprintMaterial != null)
        {
            Color color = fingerprintMaterial.color;
            color.a = 1f; // Force full opacity.
            fingerprintMaterial.color = color;
            Renderer rend = fingerprintObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = color;
            }
        }
        if (fingerprintObject != null)
            fingerprintObject.SetActive(true);
    }

    // Triggers haptic feedback on all detected controllers.
    private void TriggerHaptics(float amplitude, float duration)
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDeviceCharacteristics controllerCharacteristics =
            UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.HeldInHand;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);
        foreach (var device in devices)
        {
            if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
            {
                uint channel = 0; // Most devices use channel 0.
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
        get { return brushPowdered && fingerprintBrushed; }
    }
}
