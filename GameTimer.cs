using UnityEngine;
using TMPro; // For TextMeshProUGUI

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float totalTime = 1200f; // 20 minutes in seconds
    private float timeRemaining;
    private bool isTimerRunning = true;

    [Header("UI References")]
    public TextMeshProUGUI timerText; // UI Text to display the countdown

    private void Start()
    {
        timeRemaining = totalTime;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                EndSimulation();
            }
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Called when the timer runs out.
    /// Finalizes assessments, displays overall and individual reports, and triggers the guide UI.
    /// </summary>
private void EndSimulation()
{
    isTimerRunning = false;
    Debug.Log("EndSimulation() called.");

    // Finalize Task1 subtasks.
    Task1Manager t1Manager = FindObjectOfType<Task1Manager>();
    if (t1Manager != null)
    {
        Debug.Log("Calling FinalizeTaskSubtasks() from GameTimer.");
        t1Manager.FinalizeTaskSubtasks();
    }
    else
    {
        Debug.LogWarning("No Task1Manager found in the scene.");
    }

    // Finalize any remaining assessments.
    AssessmentController.Instance.FinalizeAssessments();

    // Generate the overall assessment report.
    string overallReport = AssessmentController.Instance.GenerateOverallReport();
    Debug.Log("Overall Report: " + overallReport);

    // Update the UI.
    AssessmentReportUI reportUI = FindObjectOfType<AssessmentReportUI>();
    if (reportUI != null)
    {
        reportUI.DisplayReport(overallReport);         // Updates overall report (overallScoreText)
        reportUI.DisplayIndividualReports();             // Updates individual task UI fields
    }
    else
    {
        Debug.LogWarning("No AssessmentReportUI found in the scene.");
    }

    // Optionally, trigger your guide UI.
    ShowGuideCanvas guide = FindObjectOfType<ShowGuideCanvas>();
    if (guide != null)
    {
        guide.ShowGuide();
    }
    else
    {
        Debug.LogWarning("No ShowGuideCanvas found in the scene.");
    }
}

}
