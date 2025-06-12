using UnityEngine;

public class L_TaskTrigger : MonoBehaviour
{
    public bool isTriggered = false; // Indicates whether this trigger has been activated.
    private L_Task1Manager L_task1Manager; // Reference to L_Task1Manager.

    private void Start()
    {
        // Find and assign the L_Task1Manager from the scene.
        L_task1Manager = FindObjectOfType<L_Task1Manager>();
        if (L_task1Manager == null)
        {
            Debug.LogError("L_Task1Manager not found in the scene!");
        }
    }

    // When an object enters the trigger collider.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the Player (or adjust the tag as necessary).
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            Debug.Log($"L_TaskTrigger '{gameObject.name}' activated by {other.gameObject.name}.");

            // Notify the L_Task1Manager that this trigger has been activated.
            if (L_task1Manager != null)
            {
                L_task1Manager.TriggerEntered(this);
            }
        }
    }

    // Optional: Manually activate the trigger.
    public void Activate()
    {
        if (!isTriggered)
        {
            isTriggered = true;
            Debug.Log($"L_TaskTrigger '{gameObject.name}' manually activated.");

            if (L_task1Manager != null)
            {
                L_task1Manager.TriggerEntered(this);
            }
        }
    }
}
