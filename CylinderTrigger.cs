using UnityEngine;

public class CylinderTrigger : MonoBehaviour
{
    public bool isTriggered = false; // To mark when a capture has been made
    public bool playerInside = false; // To check if the player is inside the trigger
    private Task1Manager task1Manager; // Reference to Task1Manager
    public MeshRenderer associatedMeshRenderer; // Assign the MeshRenderer in the Inspector or find automatically
    public BoxCollider captureBoxCollider; // Public attribute for the BoxCollider capture area.

    private void Start()
    {
        task1Manager = FindObjectOfType<Task1Manager>();
        if (task1Manager == null)
        {
            Debug.LogError("Task1Manager is not found in the scene!");
        }

        if (associatedMeshRenderer == null)
        {
            associatedMeshRenderer = GetComponent<MeshRenderer>();
            if (associatedMeshRenderer == null)
            {
                Debug.LogError($"MeshRenderer is not found on or assigned for {gameObject.name}!");
            }
        }

        if (captureBoxCollider == null)
        {
            captureBoxCollider = GetComponent<BoxCollider>();
            if (captureBoxCollider == null)
            {
                Debug.LogError($"BoxCollider is not found on or assigned for {gameObject.name}!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("Player entered the cylinder trigger.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("Player exited the cylinder trigger.");
        }
    }

    public void Capture()
    {
        if (playerInside && !isTriggered)
        {
            isTriggered = true;
            Debug.Log("Capture made inside the cylinder trigger.");

            if (task1Manager != null)
            {
                task1Manager.TriggerEntered(this);
            }

            if (associatedMeshRenderer != null)
            {
                associatedMeshRenderer.enabled = false;
                Debug.Log($"MeshRenderer on {gameObject.name} has been disabled.");
            }
        }
        else
        {
            Debug.LogWarning("Capture attempted outside the cylinder trigger or already triggered.");
            //AssessmentController.Instance.LogMistake("Task1", "Capture attempted outside the designated area.", 5f, "Ensure you're fully inside the capture area before capturing.");
        }
    }
}
