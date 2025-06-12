using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class L_FingerprintManager : MonoBehaviour
{
    // Reference to the L_TaskManagerController2 to notify task completion.
    public L_TaskManagerController2 taskManagerController;
    
    // UI elements for header and task toggle display.
    public TMP_Text headerText;
    public Transform tasksContainer;
    public GameObject taskTogglePrefab;
    
    // The flashlight used to detect the fingerprint.
    public Light flashlight;

    private FingerprintInfo fingerprint; // Holds fingerprint details.
    private bool taskCompleted = false;   // Task completion flag.
    private bool isDiscovered = false;    // Ensures discovery is triggered only once.

    // *** Added variables for timer logic ***
    private float flashTimer = 0f;         // Tracks how long the flashlight is aimed correctly.
    private const float requiredFlashTime = 2f; // Required continuous time in seconds.

    [System.Serializable]
    public class FingerprintInfo
    {
        public string fingerprintName;
        public GameObject fingerprintObject; // Fingerprint GameObject.
        public Material fingerprintMaterial; // Used to adjust transparency.
        public bool isActivated;             // Whether the fingerprint is active.
    }

    // Fingerprint information provided via the Inspector.
    public FingerprintInfo fingerprintInfo;

    // Angle threshold to detect if the flashlight is pointing correctly.
    public float detectionAngle = 10f;

    public TaskTransitionManager2 taskTransitionManager2;

    void Start()
    {
        fingerprint = fingerprintInfo;
        fingerprint.isActivated = false;

        // Initially hide the fingerprint and set its transparency to invisible.
        fingerprint.fingerprintObject.SetActive(false);
        SetFingerprintVisibility(false);

        // Dynamically create a UI toggle for this fingerprint task.
        CreateFingerprintToggle(fingerprint.fingerprintName);

        // Update the header to reflect the initial state.
        UpdateHeader();
    }

    // Dynamically create a toggle UI element for the fingerprint task.
    private void CreateFingerprintToggle(string fingerprintName)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasksContainer);
        TMP_Text toggleText = toggleObject.GetComponentInChildren<TMP_Text>();
        toggleText.text = fingerprintName + " (Inactive)";

        // Ensure the toggle is not interactable and starts unchecked.
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.interactable = false;
        toggle.isOn = false;
    }

    void Update()
    {
        // Only check detection if the flashlight is enabled.
        if (flashlight.enabled)
        {
            CheckForActivatedFingerprint();
        }
        else
        {
            // When flashlight is off, ensure fingerprint is hidden and reset timer.
            fingerprint.fingerprintObject.SetActive(false);
            SetFingerprintVisibility(false);
            flashTimer = 0f; // *** Added: Reset timer when flashlight is off ***
        }
    }

    // Checks if the flashlight is correctly pointed at the fingerprint.
    private void CheckForActivatedFingerprint()
    {
        if (IsFlashlightPointingAt(fingerprint.fingerprintObject))
        {
            // Make the fingerprint visible while the flashlight is pointing at it.
            fingerprint.fingerprintObject.SetActive(true);
            SetFingerprintVisibility(true);
            
            // *** Added: Increment timer and check if it meets required duration ***
            flashTimer += Time.deltaTime;
            if (flashTimer >= requiredFlashTime && !isDiscovered)
            {
                isDiscovered = true;
                taskCompleted = true;
                ActivateFingerprint(fingerprint);
            }
        }
        else
        {
            // *** Added: Reset timer if flashlight is not aimed correctly ***
            flashTimer = 0f;
            fingerprint.fingerprintObject.SetActive(false);
            SetFingerprintVisibility(false);
        }

        // Always update the header based on current state.
        UpdateHeader();
    }

    // Returns true if the flashlight's forward direction is within the detection angle of the fingerprint.
    private bool IsFlashlightPointingAt(GameObject fingerprintObject)
    {
        Vector3 directionToFingerprint = (fingerprintObject.transform.position - flashlight.transform.position).normalized;
        float angle = Vector3.Angle(flashlight.transform.forward, directionToFingerprint);
        return angle <= detectionAngle;
    }

    // Called when the fingerprint is activated (first discovered).
    private void ActivateFingerprint(FingerprintInfo fingerprint)
    {
        if (!fingerprint.isActivated)
        {
            fingerprint.isActivated = true;

            // Update the UI toggle to indicate this fingerprint is now active.
            foreach (Transform child in tasksContainer)
            {
                TMP_Text toggleText = child.GetComponentInChildren<TMP_Text>();
                if (toggleText.text.Contains(fingerprint.fingerprintName))
                {
                    toggleText.text = fingerprint.fingerprintName + " (Active)";
                    child.GetComponent<Toggle>().isOn = true;
                    break;
                }
            }

            Debug.Log($"Fingerprint '{fingerprint.fingerprintName}' found after {requiredFlashTime} seconds of flashing!");
            taskTransitionManager2.PostTask1Transition();
            UpdateHeader();

            // Notify the task manager that this task is complete.
            taskManagerController.CompleteTask();
        }
    }

    // Adjusts the transparency of the fingerprint material.
    private void SetFingerprintVisibility(bool isVisible)
    {
        if (fingerprint.fingerprintMaterial != null)
        {
            Color color = fingerprint.fingerprintMaterial.color;
            color.a = isVisible ? 1f : 0f;
            fingerprint.fingerprintMaterial.color = color;
        }
    }

    // Updates the header UI to reflect the task status.
    public void UpdateHeader()
    {
        if (taskCompleted)
        {
            headerText.text = "<color=green>(COMPLETE)</color> The fingerprint has been identified.";
        }
        else
        {
            headerText.text = "<color=red>(INCOMPLETE)</color> Find the fingerprint using the flashlight.";
        }
    }

    // Returns the number of tasks in this module (only one fingerprint task).
    public int GetTotalTasks()
    {
        return 1;
    }

    // Property to expose the task completion status.
    public bool TaskCompleted
    {
        get { return taskCompleted; }
    }
}
