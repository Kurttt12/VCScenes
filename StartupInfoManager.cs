using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Shows a startup info canvas and unpauses the game after a delay.
/// </summary>
public class StartupInfoManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Canvas displaying startup information")]
    public GameObject infoCanvas;

    [Tooltip("TextMeshProUGUI component displaying countdown in seconds")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Time in seconds before the game resumes")]
    public float delayBeforeUnpause = 3f;

    private void Start()
    {
        // Pause the game
        Time.timeScale = 0f;

        // Show startup canvas
        if (infoCanvas != null) infoCanvas.SetActive(true);

        // Start coroutine to unpause after delay
        StartCoroutine(UnpauseAfterDelay());
    }

    private IEnumerator UnpauseAfterDelay()
    {
        float remainingTime = delayBeforeUnpause;

        while (remainingTime > 0f)
        {
            if (countdownText != null)
                countdownText.text = Mathf.Ceil(remainingTime).ToString();

            yield return new WaitForSecondsRealtime(1f);
            remainingTime -= 1f;
        }

        // Hide canvas
        if (infoCanvas != null) infoCanvas.SetActive(false);

        // Resume game
        Time.timeScale = 1f;
    }
}
