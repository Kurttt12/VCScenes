using System.Collections.Generic;
using UnityEngine;

public class AssessmentController : MonoBehaviour
{
    // Singleton instance for global access.
    public static AssessmentController Instance { get; private set; }

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
        public bool wasAttempted = false;
        public List<MistakeRecord> mistakeRecords = new List<MistakeRecord>();

        public TaskAssessmentData(float maxScore)
        {
            this.maxScore = maxScore;
            this.currentScore = maxScore;
        }
    }

    private Dictionary<string, TaskAssessmentData> taskAssessments = new Dictionary<string, TaskAssessmentData>();
    public string[] expectedTasks = new string[] { "Task1", "Task2", "Task3" };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        var data = taskAssessments[taskId];
        int neatDeduction = Mathf.RoundToInt(deduction);
        data.mistakeRecords.Add(new MistakeRecord(description, neatDeduction, tip));
        data.currentScore = Mathf.Max(0, data.currentScore - neatDeduction);
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
        taskAssessments[taskId].wasAttempted = true;
        Debug.Log($"[{taskId}] Success: {message}");
    }

    public void FinalizeAssessments()
    {
        foreach (var taskId in expectedTasks)
        {
            if (!taskAssessments.ContainsKey(taskId))
            {
                InitializeTaskAssessment(taskId, 100f);
                LogMistake(taskId, "Task was not attempted.", 100f, "Make sure to complete this task next time.");
            }
            else if (!taskAssessments[taskId].wasAttempted)
            {
                LogMistake(taskId, "Task capturing was not made.", defaultMissPenalty, "You must capture the scene.");
            }
        }
    }

    public string GetMistakesForTask(string taskId)
    {
        if (!taskAssessments.ContainsKey(taskId))
            return "No capturing data available.";

        var aggregated = new Dictionary<string, (int count, float totalDeduction, string tip)>();
        foreach (var rec in taskAssessments[taskId].mistakeRecords)
        {
            if (aggregated.ContainsKey(rec.description))
            {
                var ex = aggregated[rec.description];
                ex.count++;
                ex.totalDeduction += rec.deduction;
                aggregated[rec.description] = ex;
            }
            else
            {
                aggregated[rec.description] = (1, rec.deduction, rec.tip);
            }
        }

        string output = "";
        foreach (var kvp in aggregated)
            output += $"({kvp.Value.count}x) {kvp.Key} (Total Deduction: {kvp.Value.totalDeduction})\n";

        return string.IsNullOrEmpty(output) ? "No mistakes." : output;
    }

    public string GetTipsForTask(string taskId)
    {
        if (!taskAssessments.ContainsKey(taskId))
            return "No tips available.";

        var uniqueTips = new HashSet<string>();
        foreach (var rec in taskAssessments[taskId].mistakeRecords)
            if (!string.IsNullOrEmpty(rec.tip))
                uniqueTips.Add(rec.tip);

        string tipsOutput = "";
        foreach (var tip in uniqueTips)
            tipsOutput += $"- {tip}\n";

        return string.IsNullOrEmpty(tipsOutput) ? "No tips available." : tipsOutput;
    }

    public string GetGradeForTask(string taskId)
    {
        if (!taskAssessments.ContainsKey(taskId))
            return "N/A";
        var d = taskAssessments[taskId];
        return $"{d.currentScore}/{d.maxScore}";
    }

    public string GetOverallFinalScore()
    {
        float totalMax = 0, totalScore = 0;
        foreach (var t in expectedTasks)
        {
            if (taskAssessments.ContainsKey(t))
            {
                totalMax += taskAssessments[t].maxScore;
                totalScore += taskAssessments[t].currentScore;
            }
        }
        return $"Overall Score: {totalScore}/{totalMax}";
    }

    public string GetOverallAveragePercentageScore()
    {
        float sumPct = 0;
        int count = expectedTasks.Length;
        foreach (var t in expectedTasks)
            if (taskAssessments.ContainsKey(t))
                sumPct += taskAssessments[t].currentScore / taskAssessments[t].maxScore;

        return $"{(sumPct / count * 100f):F2}%";
    }

    // New: remarks based on overall percentage
    public string GetOverallRemarks()
    {
        float sumPct = 0;
        int count = expectedTasks.Length;
        foreach (var t in expectedTasks)
            if (taskAssessments.ContainsKey(t))
                sumPct += taskAssessments[t].currentScore / taskAssessments[t].maxScore;

        float avg = sumPct / count * 100f;
        return avg > 50f ? "Passed" : "Failed";
    }

    public string GenerateFinalReportForTask(string taskId)
    {
        string report = $"=== {taskId} Report ===\n";
        report += "Mistakes:\n" + GetMistakesForTask(taskId) + "\n";
        report += "Tips:\n" + GetTipsForTask(taskId) + "\n";
        report += "Grade: " + GetGradeForTask(taskId) + "\n";
        return report;
    }

    public string GenerateOverallReport()
    {
        Task1Manager t1 = FindObjectOfType<Task1Manager>();
        if (t1 != null)
            t1.FinalizeTaskSubtasks();

        FinalizeAssessments();
        return GetOverallAveragePercentageScore();
    }
}
