using UnityEngine;

public class PauseMenuPositioner : MonoBehaviour
{
    public Transform playerCamera; // Reference to the player's camera
    public float distanceFromCamera = 1.5f; // Distance in front of the camera

    public void PositionMenu()
    {
        if(playerCamera == null)
        {
            Debug.LogWarning("PlayerCamera is not assigned in PauseMenuPositioner!");
            return;
        }

        // Position the menu in front of the player
        Vector3 newPosition = playerCamera.position + playerCamera.forward * distanceFromCamera;
        transform.position = newPosition;

        // Rotate the menu to face the player using an explicit up vector.
        transform.rotation = Quaternion.LookRotation(transform.position - playerCamera.position, Vector3.up);

        Debug.Log($"PauseMenuPositioner: Menu positioned at {newPosition} with rotation {transform.rotation.eulerAngles}");
    }
}
