using UnityEngine;

public class RayInteractorTrigger : MonoBehaviour
{
    // Public reference for the BoxCollider
    public BoxCollider triggerCollider;

    // Public references for left and right ray interactor UI GameObjects
    public GameObject leftRayInteractorUI;
    public GameObject rightRayInteractorUI;

    private void Start()
    {
        // If no collider was assigned, attempt to get one attached to this GameObject.
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<BoxCollider>();
        }

        // Ensure the BoxCollider is set as a trigger.
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
        }

        // Ensure both UIs are hidden at the start.
        if (leftRayInteractorUI != null)
        {
            leftRayInteractorUI.SetActive(false);
        }
        if (rightRayInteractorUI != null)
        {
            rightRayInteractorUI.SetActive(false);
        }
    }

    // When a collider enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            if (leftRayInteractorUI != null)
            {
                leftRayInteractorUI.SetActive(true);
            }
            if (rightRayInteractorUI != null)
            {
                rightRayInteractorUI.SetActive(true);
            }
        }
    }

    // When a collider exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            if (leftRayInteractorUI != null)
            {
                leftRayInteractorUI.SetActive(false);
            }
            if (rightRayInteractorUI != null)
            {
                rightRayInteractorUI.SetActive(false);
            }
        }
    }
}
