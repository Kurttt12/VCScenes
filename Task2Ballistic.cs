using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Task2Ballistic : MonoBehaviour
{
    public TaskManagerController taskManagerController;
    public TMP_Text headerText;
    public Transform tasksContainer;
    public GameObject taskTogglePrefab;
    private int currentTaskIndex = 0;
    private List<TaskInfo> tasks = new List<TaskInfo>();
    public bool taskCompleted = false;

    [System.Serializable]
    public struct TaskInfo
    {
        public string taskName;
        public TaskTrigger taskTrigger;
        public Toggle taskToggle;
        public List<CylinderTrigger> cylinderTriggers;
    }

    public TaskInfo[] taskInfoArray;

    void Start()
    {
        foreach (var taskInfo in taskInfoArray)
        {
            TaskInfo newTask = taskInfo;
            newTask.taskToggle = CreateTaskToggle(taskInfo.taskName);
            tasks.Add(newTask);
        }

        UpdateTaskUI();
        UpdateHeader();
        ActivateNextTask();
    }

    private Toggle CreateTaskToggle(string taskName)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasksContainer);
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        int totalCount = taskInfoArray[currentTaskIndex].cylinderTriggers.Count;
        toggle.GetComponentInChildren<TMP_Text>().text = $"{taskName} (0/{totalCount})";
        toggle.interactable = false;
        return toggle;
    }

    private void UpdateTaskUI()
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].taskToggle.isOn = (i < currentTaskIndex);
            int triggeredCount = taskInfoArray[i].cylinderTriggers.Count(t => t.isTriggered);
            int totalCount = taskInfoArray[i].cylinderTriggers.Count;
            tasks[i].taskToggle.GetComponentInChildren<TMP_Text>().text = tasks[i].taskName + $" ({triggeredCount}/{totalCount})";
        }
    }

    public bool CompleteTask(string taskName)
    {
        if (currentTaskIndex < tasks.Count && tasks[currentTaskIndex].taskName == taskName)
        {
            tasks[currentTaskIndex].taskToggle.isOn = true;
            currentTaskIndex++;
            
            if (currentTaskIndex >= tasks.Count)
            {
                taskCompleted = true;
                taskManagerController.CompleteTask();
            }
            
            UpdateTaskUI();
            UpdateHeader();
            ActivateNextTask();
            return true;
        }
        return false;
    }

    private void ActivateNextTask()
    {
        if (currentTaskIndex < tasks.Count)
        {
            tasks[currentTaskIndex].taskTrigger.gameObject.SetActive(true);
        }
    }

    public void UpdateHeader()
    {
        headerText.text = taskCompleted
            ? "<color=green>(COMPLETE)</color> Second ballistic task done!"
            : "<color=red>(INCOMPLETE)</color> Complete the second ballistic task!";
    }
}