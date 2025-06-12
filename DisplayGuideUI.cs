using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit; // For TeleportationProvider and TeleportRequest

public class DisplayGuideUI : MonoBehaviour
{
    public GameObject guideCanvas;         // Canvas that shows the guide when tasks are completed
    public Button proceedButton;           // Button on the guide canvas
    public PauseMenuPositioner menuPositioner; // Positions the canvas in front of the player's camera

    // Teleportation specific objects:
    public TeleportationProvider teleportationProvider;  // Handles teleportation requests
    public Transform designatedRoomTransform;            // Target transform (position & rotation) of the designated room

    private void Start()
    {
        Debug.Log("DisplayGuideUI started.");

        // Only disable guideCanvas if it's not the completion canvas.
        // Alternatively, remove this if the canvas should remain active after task completion.
        if (guideCanvas != null && !guideCanvas.name.Contains("Tasks Completed"))
            guideCanvas.SetActive(false);

        if (proceedButton != null)
            proceedButton.onClick.AddListener(OnProceedButtonClicked);
    }


    /// <summary>
    /// Displays the guide UI and then pauses the game (using near‑zero timescale).
    /// </summary>
    public void DisplayUI()
    {
        if (guideCanvas != null && !guideCanvas.activeSelf)
        {
            guideCanvas.SetActive(true);

            // Reposition the canvas in front of the player.
            if (menuPositioner != null)
                menuPositioner.PositionMenu();

            // Wait a short real-time delay then set a near‑zero timescale.
            StartCoroutine(PauseGameAfterDelay());
        }
    }

    private IEnumerator PauseGameAfterDelay()
    {
        // Wait for 0.1 seconds in real time to allow UI updates.
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 0.001f; // Nearly pause the game.
    }

    /// <summary>
    /// Called when the proceed button is clicked.
    /// Teleports the player, finalizes assessments, updates the assessment UI, and resumes the game.
    /// </summary>
    private void OnProceedButtonClicked()
    {
        Debug.Log("Proceeding after guide UI.");

        // Teleport the player to the designated room.
        if (teleportationProvider != null && designatedRoomTransform != null)
        {
            TeleportRequest teleportRequest = new TeleportRequest()
            {
                destinationPosition = designatedRoomTransform.position,
                destinationRotation = designatedRoomTransform.rotation
            };
            teleportationProvider.QueueTeleportRequest(teleportRequest);
        }
        else
        {
            Debug.LogWarning("TeleportationProvider or designatedRoomTransform is not set in DisplayGuideUI.");
        }

        // Finalize assessments.
        AssessmentController.Instance.FinalizeAssessments();
        string overallReport = AssessmentController.Instance.GenerateOverallReport();
        Debug.Log("Overall Report from DisplayGuideUI: " + overallReport);

        // Update the Assessment UI.
        AssessmentReportUI reportUI = FindObjectOfType<AssessmentReportUI>();
        if (reportUI != null)
        {
            reportUI.DisplayReport(overallReport);
            reportUI.DisplayIndividualReports();
        }
        else
        {
            Debug.LogWarning("No AssessmentReportUI found in the scene.");
        }

        // Resume the game.
        Time.timeScale = 1f;

        // Hide the guide UI.
        if (guideCanvas != null)
            guideCanvas.SetActive(false);
    }
}
