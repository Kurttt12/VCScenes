using UnityEngine;

public class TaskTrigger : MonoBehaviour
{
    public bool isTriggered = false; // Indicates whether this trigger has been activated.
    private Task1Manager task1Manager; // Reference to Task1Manager.

    private void Start()
    {
        // Find and assign the Task1Manager from the scene.
        task1Manager = FindObjectOfType<Task1Manager>();
        if (task1Manager == null)
        {
            Debug.LogError("Task1Manager not found in the scene!");
        }
    }

    // When an object enters the trigger collider.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the Player (or adjust the tag as necessary).
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            Debug.Log($"TaskTrigger '{gameObject.name}' activated by {other.gameObject.name}.");

            // Notify the Task1Manager that this trigger has been activated.
            if (task1Manager != null)
            {
                task1Manager.TriggerEntered(this);
            }
        }
    }

    // Optional: Manually activate the trigger.
    public void Activate()
    {
        if (!isTriggered)
        {
            isTriggered = true;
            Debug.Log($"TaskTrigger '{gameObject.name}' manually activated.");

            if (task1Manager != null)
            {
                task1Manager.TriggerEntered(this);
            }
        }
    }
}
