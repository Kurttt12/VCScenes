using System.Collections.Generic;
using UnityEngine;

public class L_AssessmentController : MonoBehaviour
{
    // Singleton instance for global access.
    public static L_AssessmentController Instance { get; private set; }

    // Default penalty for a task not attempted.
    public float defaultMissPenalty = 50f;

    [System.Serializable]
    public struct MistakeRecord
    {
        public string description;
        public float deduction;
        public string tip;

        public MistakeRecord(string description, float deduction, string tip)
        {
            this.description = description;
            this.deduction = deduction;
            this.tip = tip;
        }
    }

    [System.Serializable]
    public class TaskAssessmentData
    {
        public float maxScore;
        public float currentScore;
        // Flag to indicate whether any capturing (success or mistake) occurred.
        public bool wasAttempted = false;
        public List<MistakeRecord> mistakeRecords = new List<MistakeRecord>();

        public TaskAssessmentData(float maxScore)
        {
            this.maxScore = maxScore;
            this.currentScore = maxScore;
        }
    }

    // Dictionary mapping task identifiers (e.g., "Task1", "Task2", "Task3") to their assessment data.
    private Dictionary<string, TaskAssessmentData> taskAssessments = new Dictionary<string, TaskAssessmentData>();

    // Expected tasks list.
    public string[] expectedTasks = new string[] { "Task1", "Task2", "Task3" };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optionally persist between scenes:
        // DontDestroyOnLoad(gameObject);
    }

    public void InitializeTaskAssessment(string taskId, float maxScore)
    {
        if (!taskAssessments.ContainsKey(taskId))
        {
            taskAssessments[taskId] = new TaskAssessmentData(maxScore);
            Debug.Log($"Initialized assessment for {taskId} with max score {maxScore}.");
        }
    }

    public void LogMistake(string taskId, string description, float deduction, string tip)
    {
        if (!taskAssessments.ContainsKey(taskId))
        {
            Debug.LogWarning($"Task {taskId} not initialized. Initializing with default max score of 100.");
            InitializeTaskAssessment(taskId, 100f);
        }
        TaskAssessmentData data = taskAssessments[taskId];
        // Round the deduction to the nearest whole number.
        int neatDeduction = Mathf.RoundToInt(deduction);
        data.mistakeRecords.Add(new MistakeRecord(description, neatDeduction, tip));
        data.currentScore = Mathf.Max(0, data.currentScore - neatDeduction); // Clamp score to a minimum of 0
        data.wasAttempted = true;
        Debug.Log($"[{taskId}] Logged mistake: {description} (Deduction: {neatDeduction}). Tip: {tip}");
    }

    public void LogSuccess(string taskId, string message)
    {
        if (!taskAssessments.ContainsKey(taskId))
        {
            Debug.LogWarning($"Task {taskId} not initialized. Initializing with default max score of 100.");
            InitializeTaskAssessment(taskId, 100f);
        }
        // Mark the task as attempted even if no mistake is recorded.
        taskAssessments[taskId].wasAttempted = true;
        Debug.Log($"[{taskId}] Success: {message}");
    }

    /// <summary>
    /// Finalizes assessments by ensuring every expected task was attempted.
    /// If a task was not attempted, logs a default mistake.
    /// </summary>
    public void FinalizeAssessments()
    {
        foreach (var taskId in expectedTasks)
        {
            // If the task hasn't been initialized or was never attempted:
            if (!taskAssessments.ContainsKey(taskId))
            {
                InitializeTaskAssessment(taskId, 100f);
                LogMistake(taskId, "Task was not attempted.", 100f, "Make sure to complete this task next time.");
            }
            else if (!taskAssessments[taskId].wasAttempted)
            {
                // Task was initialized but not attempted.
                LogMistake(taskId, "Task capturing was not made.", defaultMissPenalty, "You must capture the scene.");
            }
        }
    }

    /// <summary>
    /// Returns an aggregated string of mistakes for the specified task.
    /// Each unique mistake is listed once with its count and total deduction.
    /// If no mistakes are recorded, returns a custom message.
    /// </summary>
    public string GetMistakesForTask(string taskId)
    {
        if (!taskAssessments.ContainsKey(taskId))
            return "No capturing data available.";

        // Aggregate duplicate mistakes.
        Dictionary<string, (int count, float totalDeduction, string tip)> aggregatedMistakes = new Dictionary<string, (int, float, string)>();

        foreach (var record in taskAssessments[taskId].mistakeRecords)
        {
            if (aggregatedMistakes.ContainsKey(record.description))
            {
                var existing = aggregatedMistakes[record.description];
                existing.count++;
                existing.totalDeduction += record.deduction;
                aggregatedMistakes[record.description] = existing;
            }
            else
            {
                aggregatedMistakes[record.description] = (1, record.deduction, record.tip);
            }
        }

        string output = "";
        foreach (var kvp in aggregatedMistakes)
        {
            output += $"({kvp.Value.count}x) {kvp.Key} (Total Deduction: {kvp.Value.totalDeduction})\n";
        }
        // Return "No mistakes." if nothing was recorded.
        return string.IsNullOrEmpty(output) ? "No mistakes." : output;
    }

    /// <summary>
    /// Returns a string of unique tips for the specified task.
    /// </summary>
    public string GetTipsForTask(string taskId)
    {
        if (!taskAssessments.ContainsKey(taskId))
            return "No tips available.";

        HashSet<string> uniqueTips = new HashSet<string>();
        foreach (var record in taskAssessments[taskId].mistakeRecords)
        {
            if (!string.IsNullOrEmpty(record.tip))
                uniqueTips.Add(record.tip);
        }
        string tipsOutput = "";
        foreach (var tip in uniqueTips)
        {
            tipsOutput += $"- {tip}\n";
        }
        return string.IsNullOrEmpty(tipsOutput) ? "No tips available." : tipsOutput;
    }

    public string GetGradeForTask(string taskId)
    {
        if (!taskAssessments.ContainsKey(taskId))
            return "N/A";
        TaskAssessmentData data = taskAssessments[taskId];
        return $"{data.currentScore}/{data.maxScore}";
    }

    /// <summary>
    /// Returns a formatted string showing the overall final score.
    /// </summary>
    public string GetOverallFinalScore()
    {
        float totalMax = 0f;
        float totalScore = 0f;
        foreach (var taskId in expectedTasks)
        {
            if (taskAssessments.ContainsKey(taskId))
            {
                totalMax += taskAssessments[taskId].maxScore;
                totalScore += taskAssessments[taskId].currentScore;
            }
        }
        return $"Overall Score: {totalScore}/{totalMax}";
    }

    public string GenerateFinalReportForTask(string taskId)
    {
        string report = $"=== {taskId} Report ===\n";
        report += "Mistakes:\n" + GetMistakesForTask(taskId) + "\n";
        report += "Tips:\n" + GetTipsForTask(taskId) + "\n";
        report += "Grade: " + GetGradeForTask(taskId) + "\n";
        return report;
    }

    public string GetOverallAveragePercentageScore()
    {
        float sumPercentage = 0f;
        int count = expectedTasks.Length;
        foreach (var taskId in expectedTasks)
        {
            if (taskAssessments.ContainsKey(taskId))
            {
                sumPercentage += taskAssessments[taskId].currentScore / taskAssessments[taskId].maxScore;
            }
            else
            {
                // If a task wasn't even initialized, treat it as 0%.
                sumPercentage += 0f;
            }
        }
        float average = (sumPercentage / count) * 100f;
        return $"{average:F2}%";
    }


public string GenerateOverallReport()
{
    // === NEW: run Task1 subtask finalization first ===
    Task1Manager t1 = FindObjectOfType<Task1Manager>();
    if (t1 != null)
        t1.FinalizeTaskSubtasks();
    // ==================================================

    // Now run the existing per‐task checks (Task1, Task2, Task3)
    FinalizeAssessments();
    return GetOverallAveragePercentageScore();
}

}
