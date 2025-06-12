using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public TutorialManager tutorialManager; // Reference to the TutorialManager script
    public bool isTriggered = false; // Track if the trigger has been activated

    private void Start()
    {
        // Ensure the TutorialManager is assigned
        if (tutorialManager == null)
        {
            Debug.LogError($"TutorialManager is not assigned on {gameObject.name}!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger area
        if (other.CompareTag("Player") && !isTriggered)
        {
            if (tutorialManager != null)
            {
                tutorialManager.TriggerEntered(this);
            }
        }
    }

    public void MarkAsTriggered()
    {
        isTriggered = true;
        gameObject.SetActive(false); // Disable the trigger once marked as complete
    }

    public void ReactivateTrigger()
    {
        isTriggered = false; // Reset the trigger status
        gameObject.SetActive(true); // Reactivate the trigger
    }
}
