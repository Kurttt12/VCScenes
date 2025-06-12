using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class L_Task3Manager : MonoBehaviour
{
    public L_TaskManagerController L_taskManagerController;  // Reference to the L_TaskManagerController
    public TMP_Text headerText;         // UI element for header (incomplete/complete status)
    public TMP_Text headerNextText;     // UI element for header when this task is next
    public Transform tasks3Container;   // Container for task toggles for Task 3
    public GameObject taskTogglePrefab; // Prefab for task toggles

    public TaskTransitionManager taskTransitionManager;

    // Array of L_Evidence objects.
    // Each L_Evidence should have:
    //   • isCaptured (for the initial capture)
    //   • miniTaskTriggers (List<BoxTrigger>) for the scale marker placement (step 2)
    //   • finalCaptureDetector (FinalCaptureDetector) for the final capture (step 3)
    public L_Evidence[] L_evidences;

    // (Optional) A general scale marker object if needed.
    public GameObject scaleMarkerObject;

    public bool taskCompleted = false; // Overall task completion flag

    // Dictionary mapping each L_Evidence name to its UI toggle.
    private Dictionary<string, Toggle> L_evidenceToggles = new Dictionary<string, Toggle>();

    // Dictionary to flag if an L_evidence’s full (3/3) progress has been logged.
    private Dictionary<string, bool> L_evidenceCompletionLogged = new Dictionary<string, bool>();

    void Start()
    {
        Debug.Log("Initializing L_Task3Manager for L_evidences...");
        // Initialize Task3 assessment data.
        AssessmentController.Instance.InitializeTaskAssessment("Task3", 100f);

        // Create a toggle for each L_Evidence.
        for (int i = 0; i < L_evidences.Length; i++)
        {
            Toggle toggle = CreateTaskToggle(L_evidences[i].name, L_evidences[i]);
            L_evidenceToggles.Add(L_evidences[i].name, toggle);
            // Mark each L_evidence as not yet logged.
            L_evidenceCompletionLogged[L_evidences[i].name] = false;
        }
        UpdateTaskUI();
        UpdateHeader();
    }

    // Returns true if an L_evidence’s three required steps are complete.
    private bool IsL_EvidenceComplete(L_Evidence ev)
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

    // Helper method: check if L_evidence is fully within the main camera's view.
    public bool IsL_EvidenceInFrame(L_Evidence ev)
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
            Debug.LogWarning("L_Evidence does not have a Collider.");
            return false;
        }
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, col.bounds);
    }

    private Toggle CreateTaskToggle(string L_evidenceName, L_Evidence ev)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasks3Container);
        Toggle toggle = toggleObject.GetComponent<Toggle>();

        // Calculate total steps: 1 for initial capture + mini tasks + 1 for final capture (if applicable).
        int totalCount = 1;
        if (ev.miniTaskTriggers != null)
            totalCount += ev.miniTaskTriggers.Count;
        if (ev.finalCaptureDetector != null)
            totalCount += 1;

        toggle.GetComponentInChildren<TMP_Text>().text = L_evidenceName + " (0/" + totalCount + ")";
        // Keep the toggle visible as this task is non-sequential.
        toggle.interactable = true;
        toggle.isOn = false;
        return toggle;
    }

    void Update()
    {
        if (L_taskManagerController.IsCurrentTask(this.gameObject))
        {
            UpdateTaskUI();
            UpdateHeader();
            // Once all L_evidences are finished, mark the task as completed.
            if (L_evidences.All(ev => IsL_EvidenceComplete(ev)) && !taskCompleted)
            {
                taskCompleted = true;
                Debug.Log("Task 'Capture L_Evidences' fully completed.");
                AssessmentController.Instance.LogSuccess("Task3", "Task 'Capture L_Evidences' completed successfully.");
                // Notify the controller that Task3 is complete.
                taskTransitionManager.PostTask3Transition();
                L_taskManagerController.CompleteTask();
            }
        }
        // Always update header (so that if taskCompleted is set, header reflects the correct status)
        UpdateHeader();
    }

    private void UpdateTaskUI()
    {
        bool isCurrentTask = L_taskManagerController.IsCurrentTask(this.gameObject);
        // Update the toggle for each L_evidence.
        for (int i = 0; i < L_evidences.Length; i++)
        {
            string name = L_evidences[i].name;
            if (L_evidenceToggles.TryGetValue(name, out Toggle toggle))
            {
                toggle.gameObject.SetActive(isCurrentTask);
                toggle.interactable = true;

                // Compute how many substeps have been completed.
                int completedCount = 0;
                if (L_evidences[i].isCaptured)
                    completedCount++;
                if (L_evidences[i].miniTaskTriggers != null)
                    completedCount += L_evidences[i].miniTaskTriggers.Count(t => t.isTriggered);
                if (L_evidences[i].finalCaptureDetector != null && L_evidences[i].finalCaptureDetector.isFinalCaptured)
                    completedCount++;

                int totalCount = 1 + (L_evidences[i].miniTaskTriggers != null ? L_evidences[i].miniTaskTriggers.Count : 0)
                    + (L_evidences[i].finalCaptureDetector != null ? 1 : 0);
                toggle.GetComponentInChildren<TMP_Text>().text = L_evidences[i].name + " (" + completedCount + "/" + totalCount + ")";
                
                // Toggle "on" if fully complete.
                toggle.isOn = (completedCount == totalCount);

                // When an L_evidence finishes all three steps (3/3) and hasn’t been logged, update progress.
                if (toggle.isOn && !L_evidenceCompletionLogged[name])
                {
                    L_evidenceCompletionLogged[name] = true;  // Mark as logged.
                    Debug.Log("L_Evidence '" + name + "' completed (3/3). Incrementing progress.");
                    L_taskManagerController.CompleteTask(); // Report this L_evidence as a finished unit.
                }
            }
        }
    }

    public void UpdateHeader()
    {
        // If this task is currently active (and not complete), show as INCOMPLETE;
        // otherwise, if this task is complete, show as COMPLETE.
        if (L_taskManagerController.IsCurrentTask(this.gameObject))
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
    public void OnL_EvidenceCaptured(GameObject L_evidenceGameObject)
    {
        L_Evidence ev = L_evidenceGameObject.GetComponent<L_Evidence>();
        if (ev == null)
        {
            Debug.Log("No L_Evidence component found on the captured target.");
            return;
        }
        // Check that the L_evidence is fully in frame.
        if (!IsL_EvidenceInFrame(ev))
        {
            Debug.Log("L_Evidence is not fully in frame for initial capture.");
            L_Notification.Instance.PlaySound("incorrect");
            L_Notification.Instance.PlaySound("incorrectevscalemarker");
            AssessmentController.Instance.LogMistake(
                "Task3",
                "L_Evidence not fully in frame during initial capture.",
                5f,
                "Adjust your view so the entire L_evidence is visible."
            );
            return;
        }
        if (!ev.isCaptured)
        {
            ev.Capture();
            Debug.Log("Initial capture completed for L_evidence: " + ev.name);
            UpdateTaskUI();
        }
        else
        {
            Debug.Log("Initial capture ignored: L_Evidence is already captured.");
        }
    }

    // Called when the final capture (step 3) is performed.
    public void OnFinalCaptureCaptured(GameObject finalCaptureObject)
    {
        L_Evidence ev = finalCaptureObject.GetComponent<L_Evidence>();
        if (ev == null)
        {
            Debug.Log("No L_Evidence component found on the captured target.");
            return;
        }
        // Check that the L_evidence is fully in frame.
        if (!IsL_EvidenceInFrame(ev))
        {
            Debug.Log("L_Evidence is not fully in frame for final capture.");
            L_Notification.Instance.PlaySound("incorrect");
            L_Notification.Instance.PlaySound("incorrectevidence");
            AssessmentController.Instance.LogMistake(
                "Task3",
                "L_Evidence not fully in frame during final capture.",
                5f,
                "Adjust your view so the entire L_evidence is visible."
            );
            return;
        }
        if (ev.finalCaptureDetector != null && !ev.finalCaptureDetector.isFinalCaptured)
        {
            ev.finalCaptureDetector.SetFinalCaptured();
            Debug.Log("Final capture completed for L_evidence: " + ev.name);
            UpdateTaskUI();
        }
        else
        {
            Debug.Log("Final capture ignored: L_Evidence final capture is already set.");
        }
    }

    // Called when a mini task (e.g. placing the scale marker) is triggered.
    public void OnMiniTaskTriggered(L_Evidence ev)
    {
        if (ev == null)
        {
            Debug.Log("No L_Evidence component found for mini task trigger.");
            return;
        }
        Debug.Log("Mini task triggered for L_evidence: " + ev.name);
        UpdateTaskUI();
    }

    public L_Evidence GetCurrentL_Evidence()
    {
        // For non-sequential use, return null or implement another selection method.
        return null;
    }
}
