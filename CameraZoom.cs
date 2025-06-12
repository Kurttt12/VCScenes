using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float[] zoomLevels = { 35f, 50f, 20f }; // Array of zoom levels
    private int currentZoomIndex = 0; // Index to track the current zoom level

    [Header("Interaction")]
    public InputActionProperty leftHandTriggerAction; // InputAction for the left hand's trigger

    [Header("Camera")]
    public Camera targetCamera; // Reference to the Camera component

    [Header("Trigger Area")]
    public Collider triggerAreaCollider; // Public reference for the trigger area collider

    [Header("Hand Collider")]
    public Collider leftHandCollider; // Public reference for the left hand collider

    private bool isTriggerPressed = false; // Track if the trigger was pressed
    private bool isInsideCollider = false; // Track if the hand is inside the collider

    void Start()
    {
        // Ensure the targetCamera is assigned
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
            {
                Debug.LogError("CameraZoom script requires a Camera component.");
                enabled = false; // Disable the script if no Camera is found
                return;
            }
        }
    }

    void Update()
    {
        // Check if the trigger is pressed and the hand is inside the collider
        float triggerValue = leftHandTriggerAction.action.ReadValue<float>();

        if (triggerValue > 0.1f && isInsideCollider && !isTriggerPressed) // Assuming trigger value ranges from 0 to 1
        {
            isTriggerPressed = true; // Set the flag to prevent continuous cycling
            CycleZoomLevel(); // Change the zoom level
        }
        else if (triggerValue <= 0.1f)
        {
            isTriggerPressed = false; // Reset the flag when the trigger is released
        }
    }

    private void CycleZoomLevel()
    {
        // Move to the next zoom level
        currentZoomIndex = (currentZoomIndex + 1) % zoomLevels.Length;

        // Set the camera's field of view to the new zoom level
        targetCamera.fieldOfView = zoomLevels[currentZoomIndex];
    }

    // Detect when the hand enters the collider
    private void OnTriggerEnter(Collider other)
    {
        if (other == leftHandCollider)
        {
            isInsideCollider = true;
        }
    }

    // Detect when the hand exits the collider
    private void OnTriggerExit(Collider other)
    {
        if (other == leftHandCollider)
        {
            isInsideCollider = false;
        }
    }
}
