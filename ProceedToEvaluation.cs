using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ShowGuideCanvas : MonoBehaviour
{
    public Canvas guideCanvas;                   // Canvas showing the guide
    public Button proceedButton;                 // Button on the guide canvas
    public PauseMenuPositioner menuPositioner;   // Positions the canvas in front of the player's camera
    public XRRayInteractor leftRayInteractor;    // Left hand XR Ray Interactor
    public XRRayInteractor rightRayInteractor;   // Right hand XR Ray Interactor

    // Teleportation specific objects:
    public TeleportationProvider teleportationProvider;  // Handles teleportation requests
    public Transform designatedRoomTransform;            // The target transform (position & rotation) of the designated room

    private void Start()
    {
        // Hide the guide canvas and disable XR interactors at start
        if (guideCanvas != null)
            guideCanvas.enabled = false;
        if (leftRayInteractor != null)
            leftRayInteractor.gameObject.SetActive(false);
        if (rightRayInteractor != null)
            rightRayInteractor.gameObject.SetActive(false);

        // Add listener for the proceed button
        if (proceedButton != null)
            proceedButton.onClick.AddListener(Proceed);
    }

    /// <summary>
    /// Call this method from your timer script when the time is up.
    /// </summary>
    public void ShowGuide()
    {
        if (guideCanvas != null && !guideCanvas.enabled)
        {
            guideCanvas.enabled = true;

            // Position the canvas in front of the player
            if (menuPositioner != null)
                menuPositioner.PositionMenu();

            if (leftRayInteractor != null)
                leftRayInteractor.gameObject.SetActive(true);
            if (rightRayInteractor != null)
                rightRayInteractor.gameObject.SetActive(true);

            // Pause the game
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Called when the proceed button is clicked.
    /// </summary>
    public void Proceed()
    {
        Debug.Log("Proceeding to evaluation and teleporting...");

        // Ensure we have the required objects assigned
        if (teleportationProvider != null && designatedRoomTransform != null)
        {
            // Create a teleport request using the designated room's transform
            TeleportRequest teleportRequest = new TeleportRequest()
            {
                destinationPosition = designatedRoomTransform.position,
                destinationRotation = designatedRoomTransform.rotation
            };

            // Queue the teleport request
            teleportationProvider.QueueTeleportRequest(teleportRequest);
        }
        else
        {
            Debug.LogWarning("TeleportationProvider or designatedRoomTransform is not set.");
        }

        // Resume the game and disable the UI
        Time.timeScale = 1f;
        if (guideCanvas != null)
            guideCanvas.enabled = false;
        if (leftRayInteractor != null)
            leftRayInteractor.gameObject.SetActive(false);
        if (rightRayInteractor != null)
            rightRayInteractor.gameObject.SetActive(false);
    }
}
