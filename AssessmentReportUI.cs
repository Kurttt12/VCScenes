// AssessmentReportUI.cs
using UnityEngine;
using TMPro;

public class AssessmentReportUI : MonoBehaviour
{
    [Header("Overall Score UI")]
    public TMP_Text overallScoreText;
    public TMP_Text remarksText; // UI field for remarks

    [Header("Individual Task UI Elements (Optional)")]
    public TMP_Text task1MistakesText;
    public TMP_Text task1TipsText;
    public TMP_Text task1GradeText;
    public TMP_Text task2MistakesText;
    public TMP_Text task2TipsText;
    public TMP_Text task2GradeText;
    public TMP_Text task3MistakesText;
    public TMP_Text task3TipsText;
    public TMP_Text task3GradeText;

    /// <summary>
    /// Displays the overall assessment report and remarks.
    /// </summary>
    public void DisplayReport(string report)
    {
        Debug.Log("DisplayReport() called with report: " + report);
        if (overallScoreText != null)
        {
            overallScoreText.text = report;
            Debug.Log("Overall report set in UI.");
        }
        else
        {
            Debug.LogWarning("Overall Score Text is not assigned in AssessmentReportUI.");
        }

        // Display remarks with color coding
        if (remarksText != null && AssessmentController.Instance != null)
        {
            string remark = AssessmentController.Instance.GetOverallRemarks();
            remarksText.text = remark;
            remarksText.color = remark == "Passed" ? Color.green : Color.red;
            Debug.Log("Overall Remarks: " + remark);
        }
        else if (remarksText == null)
        {
            Debug.LogWarning("Remarks Text is not assigned in AssessmentReportUI.");
        }
    }

    /// <summary>
    /// Updates individual UI fields for each task.
    /// </summary>
    public void DisplayIndividualReports()
    {
        if (AssessmentController.Instance == null)
        {
            Debug.LogWarning("AssessmentController instance not found.");
            return;
        }

        // Task 1
        if (task1MistakesText != null)
        {
            var m = AssessmentController.Instance.GetMistakesForTask("Task1");
            task1MistakesText.text = m;
            Debug.Log("Task1 Mistakes: " + m);
        }
        if (task1TipsText != null)
        {
            var t = AssessmentController.Instance.GetTipsForTask("Task1");
            task1TipsText.text = t;
            Debug.Log("Task1 Tips: " + t);
        }
        if (task1GradeText != null)
        {
            var g = AssessmentController.Instance.GetGradeForTask("Task1");
            task1GradeText.text = g;
            Debug.Log("Task1 Grade: " + g);
        }

        // Task 2
        if (task2MistakesText != null)
        {
            var m = AssessmentController.Instance.GetMistakesForTask("Task2");
            task2MistakesText.text = m;
            Debug.Log("Task2 Mistakes: " + m);
        }
        if (task2TipsText != null)
        {
            var t = AssessmentController.Instance.GetTipsForTask("Task2");
            task2TipsText.text = t;
            Debug.Log("Task2 Tips: " + t);
        }
        if (task2GradeText != null)
        {
            var g = AssessmentController.Instance.GetGradeForTask("Task2");
            task2GradeText.text = g;
            Debug.Log("Task2 Grade: " + g);
        }

        // Task 3
        if (task3MistakesText != null)
        {
            var m = AssessmentController.Instance.GetMistakesForTask("Task3");
            task3MistakesText.text = m;
            Debug.Log("Task3 Mistakes: " + m);
        }
        if (task3TipsText != null)
        {
            var t = AssessmentController.Instance.GetTipsForTask("Task3");
            task3TipsText.text = t;
            Debug.Log("Task3 Tips: " + t);
        }
        if (task3GradeText != null)
        {
            var g = AssessmentController.Instance.GetGradeForTask("Task3");
            task3GradeText.text = g;
            Debug.Log("Task3 Grade: " + g);
        }
    }
}
