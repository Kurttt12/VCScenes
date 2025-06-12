using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Task3Manager : MonoBehaviour
{
    public TaskManagerController taskManagerController;  // Reference to the TaskManagerController
    public TMP_Text headerText;         // UI element for header (incomplete/complete status)
    public TMP_Text headerNextText;     // UI element for header when this task is next
    public Transform tasks3Container;   // Container for task toggles for Task 3
    public GameObject taskTogglePrefab; // Prefab for task toggles

    // Array of Evidence objects.
    // Each Evidence should have:
    //   • isCaptured (for the initial capture)
    //   • miniTaskTriggers (List<BoxTrigger>) for the scale marker placement (step 2)
    //   • finalCaptureDetector (FinalCaptureDetector) for the final capture (step 3)
    public Evidence[] evidences;

    // (Optional) A general scale marker object if needed.
    public GameObject scaleMarkerObject;

    public bool taskCompleted = false; // Overall task completion flag

    // Dictionary mapping each Evidence name to its UI toggle.
    private Dictionary<string, Toggle> evidenceToggles = new Dictionary<string, Toggle>();

    // Dictionary to flag if an evidence’s full (3/3) progress has been logged.
    private Dictionary<string, bool> evidenceCompletionLogged = new Dictionary<string, bool>();

    void Start()
    {
        Debug.Log("Initializing Task3Manager for evidences...");
        // Initialize Task3 assessment data.
        AssessmentController.Instance.InitializeTaskAssessment("Task3", 100f);

        // Create a toggle for each Evidence.
        for (int i = 0; i < evidences.Length; i++)
        {
            Toggle toggle = CreateTaskToggle(evidences[i].name, evidences[i]);
            evidenceToggles.Add(evidences[i].name, toggle);
            // Mark each evidence as not yet logged.
            evidenceCompletionLogged[evidences[i].name] = false;
        }
        UpdateTaskUI();
        UpdateHeader();
    }

    // Returns true if an evidence’s three required steps are complete.
    private bool IsEvidenceComplete(Evidence ev)
    {
        int completedCount = 0;
        if (ev.isCaptured)
            completedCount++;
        if (ev.miniTaskTriggers != null)
            completedCount += ev.miniTaskTriggers.Count(t => t.isTriggered);
        if (ev.finalCaptureDetector != null && ev.finalCaptureDetector.isFinalCaptured)
            completedCount++;

        int totalCount = 1 + (ev.miniTaskTriggers != null ? ev.miniTaskTriggers.Count : 0)
            + (ev.finalCaptureDetector != null ? 1 : 0);
        return completedCount == totalCount;
    }

    // Helper method: check if evidence is fully within the main camera's view.
    public bool IsEvidenceInFrame(Evidence ev)
    {
        if (ev == null) return false;
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main camera not found.");
            return false;
        }
        Collider col = ev.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("Evidence does not have a Collider.");
            return false;
        }
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, col.bounds);
    }

    private Toggle CreateTaskToggle(string evidenceName, Evidence ev)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasks3Container);
        Toggle toggle = toggleObject.GetComponent<Toggle>();

        // Calculate total steps: 1 for initial capture + mini tasks + 1 for final capture (if applicable).
        int totalCount = 1;
        if (ev.miniTaskTriggers != null)
            totalCount += ev.miniTaskTriggers.Count;
        if (ev.finalCaptureDetector != null)
            totalCount += 1;

        toggle.GetComponentInChildren<TMP_Text>().text = evidenceName + " (0/" + totalCount + ")";
        // Keep the toggle visible as this task is non-sequential.
        toggle.interactable = true;
        toggle.isOn = false;
        return toggle;
    }

    void Update()
    {
        if (taskManagerController.IsCurrentTask(this.gameObject))
        {
            UpdateTaskUI();
            UpdateHeader();
            // Once all evidences are finished, mark the task as completed.
            if (evidences.All(ev => IsEvidenceComplete(ev)) && !taskCompleted)
            {
                taskCompleted = true;
                Debug.Log("Task 'Capture Evidences' fully completed.");
                SoundNotification.Instance.PlaySound("incorrect");
                AssessmentController.Instance.LogSuccess("Task3", "Task 'Capture Evidences' completed successfully.");
                // Notify the controller that Task3 is complete.
                taskManagerController.CompleteTask();
            }
        }
        // Always update header (so that if taskCompleted is set, header reflects the correct status)
        UpdateHeader();
    }

    private void UpdateTaskUI()
    {
        bool isCurrentTask = taskManagerController.IsCurrentTask(this.gameObject);
        // Update the toggle for each evidence.
        for (int i = 0; i < evidences.Length; i++)
        {
            string name = evidences[i].name;
            if (evidenceToggles.TryGetValue(name, out Toggle toggle))
            {
                toggle.gameObject.SetActive(isCurrentTask);
                toggle.interactable = true;

                // Compute how many substeps have been completed.
                int completedCount = 0;
                if (evidences[i].isCaptured)
                    completedCount++;
                if (evidences[i].miniTaskTriggers != null)
                    completedCount += evidences[i].miniTaskTriggers.Count(t => t.isTriggered);
                if (evidences[i].finalCaptureDetector != null && evidences[i].finalCaptureDetector.isFinalCaptured)
                    completedCount++;

                int totalCount = 1 + (evidences[i].miniTaskTriggers != null ? evidences[i].miniTaskTriggers.Count : 0)
                    + (evidences[i].finalCaptureDetector != null ? 1 : 0);
                toggle.GetComponentInChildren<TMP_Text>().text = evidences[i].name + " (" + completedCount + "/" + totalCount + ")";
                
                // Toggle "on" if fully complete.
                toggle.isOn = (completedCount == totalCount);

                // When an evidence finishes all three steps (3/3) and hasn’t been logged, update progress.
                if (toggle.isOn && !evidenceCompletionLogged[name])
                {
                    evidenceCompletionLogged[name] = true;  // Mark as logged.
                    Debug.Log("Evidence '" + name + "' completed (3/3). Incrementing progress.");
                    taskManagerController.CompleteTask(); // Report this evidence as a finished unit.
                }
            }
        }
    }

    public void UpdateHeader()
    {
        // If this task is currently active (and not complete), show as INCOMPLETE;
        // otherwise, if this task is complete, show as COMPLETE.
        if (taskManagerController.IsCurrentTask(this.gameObject))
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            // Even if it's current, if taskCompleted is true, we want to show the complete header.
            if (taskCompleted)
                headerText.text = "<color=green>(COMPLETE)</color> Evidence Documentation";
            else
                headerText.text = "<color=red>(INCOMPLETE)</color> Evidence Documentation";
        }
        else if (taskCompleted)
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            headerText.text = "<color=green>(COMPLETE)</color> Evidence Documentation";
        }
        else
        {
            headerText.gameObject.SetActive(false);
            headerNextText.gameObject.SetActive(true);
            headerNextText.text = "<color=blue>(NEXT TASK)</color> Evidence Documentation";
        }
    }

    // Called when the initial capture (step 1) is performed.
    public void OnEvidenceCaptured(GameObject evidenceGameObject)
    {
        Evidence ev = evidenceGameObject.GetComponent<Evidence>();
        if (ev == null)
        {
            Debug.Log("No Evidence component found on the captured target.");
            return;
        }
        // Check that the evidence is fully in frame.
        if (!IsEvidenceInFrame(ev))
        {
            Debug.Log("Evidence is not fully in frame for initial capture.");
            SoundNotification.Instance.PlaySound("incorrect");
            AssessmentController.Instance.LogMistake(
                "Task3",
                "Evidence not fully in frame during initial capture.",
                5f,
                "Adjust your view so the entire evidence is visible."
            );
            return;
        }
        if (!ev.isCaptured)
        {
            ev.Capture();
            Debug.Log("Initial capture completed for evidence: " + ev.name);
            UpdateTaskUI();
        }
        else
        {
            Debug.Log("Initial capture ignored: Evidence is already captured.");
        }
    }

    // Called when the final capture (step 3) is performed.
    public void OnFinalCaptureCaptured(GameObject finalCaptureObject)
    {
        Evidence ev = finalCaptureObject.GetComponent<Evidence>();
        if (ev == null)
        {
            Debug.Log("No Evidence component found on the captured target.");
            return;
        }
        // Check that the evidence is fully in frame.
        if (!IsEvidenceInFrame(ev))
        {
            Debug.Log("Evidence is not fully in frame for final capture.");
            SoundNotification.Instance.PlaySound("incorrect");
            AssessmentController.Instance.LogMistake(
                "Task3",
                "Evidence not fully in frame during final capture.",
                5f,
                "Adjust your view so the entire evidence is visible."
            );
            return;
        }
        if (ev.finalCaptureDetector != null && !ev.finalCaptureDetector.isFinalCaptured)
        {
            ev.finalCaptureDetector.SetFinalCaptured();
            Debug.Log("Final capture completed for evidence: " + ev.name);
            UpdateTaskUI();
        }
        else
        {
            Debug.Log("Final capture ignored: Evidence final capture is already set.");
        }
    }

    // Called when a mini task (e.g. placing the scale marker) is triggered.
    public void OnMiniTaskTriggered(Evidence ev)
    {
        if (ev == null)
        {
            Debug.Log("No Evidence component found for mini task trigger.");
            return;
        }
        Debug.Log("Mini task triggered for evidence: " + ev.name);
        UpdateTaskUI();
    }

    public Evidence GetCurrentEvidence()
    {
        // For non-sequential use, return null or implement another selection method.
        return null;
    }
}
