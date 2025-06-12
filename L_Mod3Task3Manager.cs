using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class L_Mod3Task3Manager : MonoBehaviour
{
    // Reference to the main Mod3TaskManagerController3
    public Mod3TaskManagerController3 taskManagerController;

    // Header UI elements for status display
    public TMP_Text headerText;    
    public TMP_Text headerNextText; 

    // Container and prefab for the task toggle UI
    public Transform tasks3Container;  
    public GameObject taskTogglePrefab;

    // Panels for displaying bullet images and analysis panels for Match/Not Match
    public GameObject image1Panel;
    public GameObject image2Panel;
    public GameObject matchPanel;
    public GameObject notMatchPanel;

    // Buttons for image navigation and analysis
    public Button image1Button;
    public Button bothImagesButton;
    public Button image2Button;
    public Button matchButton;
    public Button notMatchButton;

    // New zoom control buttons
    public Button zoomInButton;
    public Button zoomOutButton;
    public Button resetButton;

    // Content area for zooming/sizing control
    public RectTransform contentArea;

    // Task state flag and toggle reference
    public bool taskCompleted = false;
    private Toggle taskToggle;

    // Boolean flags to track required button presses
    private bool image1Pressed = false;
    private bool bothImagesPressed = false;
    private bool image2Pressed = false;
    private bool zoomInPressed = false;
    private bool zoomOutPressed = false;
    private bool resetPressed = false;
    
    // Ensure that the button-check deduction runs only once.
    private bool buttonCheckDeducted = false;

    // Ensure that Not-Match deduction is logged only once.
    private bool notMatchDeductionApplied = false;

    public TaskTransitionManager3 taskTransitionManager3;

    void Start()
    {
        Debug.Log("Initializing L_Mod3Task3Manager...");

        // Initialize Task3 in AssessmentController to ensure it's marked as attempted later.
        if (AssessmentController.Instance != null)
        {
            AssessmentController.Instance.InitializeTaskAssessment("Task3", 100f);
        }

        // Create the task toggle if a prefab and container are assigned.
        if (taskTogglePrefab != null && tasks3Container != null)
        {
            taskToggle = CreateTaskToggle("Analyze the bullet.");
        }

        // Set up button listeners for image navigation, analysis, and zoom controls.
        image1Button.onClick.AddListener(ShowImage1);
        bothImagesButton.onClick.AddListener(ShowBothImages);
        image2Button.onClick.AddListener(ShowImage2);
        matchButton.onClick.AddListener(() => { ShowMatch(); CompleteTask(); });
        notMatchButton.onClick.AddListener(() => { ShowNotMatch(); CompleteTask(); });
        zoomInButton.onClick.AddListener(OnZoomIn);
        zoomOutButton.onClick.AddListener(OnZoomOut);
        resetButton.onClick.AddListener(OnResetZoom);

        // Optionally start by showing both images.
        ShowBothImages();

        UpdateTaskUI();
        UpdateHeader();

        // Enable task-specific UI elements if Task3 is active.
        if (taskManagerController.IsCurrentTask(gameObject))
        {
            EnableTask3UI();
        }
    }

    /// <summary>
    /// Creates and returns a task toggle based on the given task name.
    /// </summary>
    private Toggle CreateTaskToggle(string taskName)
    {
        GameObject toggleObject = Instantiate(taskTogglePrefab, tasks3Container);
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.GetComponentInChildren<TMP_Text>().text = taskName;
        toggle.interactable = false; // Non-interactable until the task is active.
        return toggle;
    }

    /// <summary>
    /// Enables the task UI elements when Task3 becomes active.
    /// </summary>
    public void EnableTask3UI()
    {
        if (image1Button != null) image1Button.gameObject.SetActive(true);
        if (bothImagesButton != null) bothImagesButton.gameObject.SetActive(true);
        if (image2Button != null) image2Button.gameObject.SetActive(true);
        if (matchButton != null) matchButton.gameObject.SetActive(true);
        if (notMatchButton != null) notMatchButton.gameObject.SetActive(true);
        if (zoomInButton != null) zoomInButton.gameObject.SetActive(true);
        if (zoomOutButton != null) zoomOutButton.gameObject.SetActive(true);
        if (resetButton != null) resetButton.gameObject.SetActive(true);

        if (image1Panel != null) image1Panel.SetActive(true);
        if (image2Panel != null) image2Panel.SetActive(true);

        // Optionally, the match/not match panels are hidden until a selection is made.
        if (matchPanel != null) matchPanel.SetActive(false);
        if (notMatchPanel != null) notMatchPanel.SetActive(false);
    }

    /// <summary>
    /// Updates the task toggle visibility and state.
    /// </summary>
    public void UpdateTaskUI()
    {
        bool isActive = taskManagerController.IsCurrentTask(gameObject);
        if (taskToggle != null)
        {
            taskToggle.gameObject.SetActive(isActive);
            taskToggle.isOn = taskCompleted;
        }
        if (isActive)
        {
            EnableTask3UI();
        }
    }

    /// <summary>
    /// Updates the header to reflect whether the task is complete, incomplete (current), or pending.
    /// </summary>
    public void UpdateHeader()
    {
        bool isCurrent = taskManagerController.IsCurrentTask(gameObject);
        
        if (taskCompleted)
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            headerText.text = "<color=green>(COMPLETE)</color> Analyze the bullet.";
        }
        else if (isCurrent)
        {
            headerText.gameObject.SetActive(true);
            headerNextText.gameObject.SetActive(false);
            headerText.text = "<color=red>(INCOMPLETE)</color> Analyze the bullet.";
        }
        else
        {
            headerText.gameObject.SetActive(false);
            headerNextText.gameObject.SetActive(true);
            headerNextText.text = "<color=blue>(NEXT TASK)</color> Analyze the bullet.";
        }
    }

    // ----- Button Callback Methods with Logging & Flag Setting -----

    public void ShowImage1()
    {
        image1Pressed = true;
        Debug.Log("Image 1 button pressed in Task3.");
        image1Panel.SetActive(true);
        image2Panel.SetActive(false);
        SetFullScreen(image1Panel);
    }

    public void ShowBothImages()
    {
        bothImagesPressed = true;
        Debug.Log("Both Images button pressed in Task3.");
        image1Panel.SetActive(true);
        image2Panel.SetActive(true);
        SetSplitScreen();
    }

    public void ShowImage2()
    {
        image2Pressed = true;
        Debug.Log("Image 2 button pressed in Task3.");
        image1Panel.SetActive(false);
        image2Panel.SetActive(true);
        SetFullScreen(image2Panel);
    }

    private void OnZoomIn()
    {
        zoomInPressed = true;
        Debug.Log("Zoom In button pressed in Task3.");
    }

    private void OnZoomOut()
    {
        zoomOutPressed = true;
        Debug.Log("Zoom Out button pressed in Task3.");
    }

    private void OnResetZoom()
    {
        resetPressed = true;
        Debug.Log("Reset button pressed in Task3.");
    }

    // ----- Analysis Panel Methods -----
    public void ShowMatch()
    {
        if (matchPanel != null && notMatchPanel != null)
        {
            Debug.Log("Match option selected in Task3.");
            matchPanel.SetActive(true);
            notMatchPanel.SetActive(false);
            SetFullScreen(matchPanel);

            // Log success for Task3 so that it is marked as attempted.
            if (AssessmentController.Instance != null)
            {
                AssessmentController.Instance.LogSuccess("Task3", "User selected 'Match' correctly.");
            }
        }
        else
        {
            Debug.LogWarning("MatchPanel or NotMatchPanel not assigned.");
        }
    }

    public void ShowNotMatch()
    {
        if (matchPanel != null && notMatchPanel != null)
        {
            Debug.Log("Not Match option selected in Task3.");
            notMatchPanel.SetActive(true);
            matchPanel.SetActive(false);
            SetFullScreen(notMatchPanel);
            
            // Deduct 30 points for selecting the wrong answer, only once.
            if (!notMatchDeductionApplied && AssessmentController.Instance != null)
            {
                Debug.Log("Deducting 30 points for selecting Not-Match (wrong answer) in Task3.");
                AssessmentController.Instance.LogMistake(
                    "Task3",
                    "Not Match option selected - wrong answer.",
                    30f,
                    "Review bullet comparison criteria and select the correct answer."
                );
                notMatchDeductionApplied = true;
            }
        }
        else
        {
            Debug.LogWarning("MatchPanel or NotMatchPanel not assigned.");
        }
    }

    // ----- Completion & Assessment Check -----

    /// <summary>
    /// Completes Task3 and then checks if all required buttons were pressed.
    /// If any are missing, it deducts points based on the number of missing buttons.
    /// </summary>
    public void CompleteTask()
    {
        Debug.Log("Mod3Task3 CompleteTask() called...");
        if (!taskCompleted && taskManagerController.IsCurrentTask(gameObject))
        {
            taskCompleted = true;
            Debug.Log("Mod3Task3 completed. Notifying controller...");
            if (taskToggle != null)
                taskToggle.isOn = true;

            UpdateTaskUI();
            UpdateHeader();

            // Disable confirmation buttons to avoid multiple calls
            matchButton.interactable = false;
            notMatchButton.interactable = false;

            // Check required button presses and deduct score if needed (only once).
            if (!buttonCheckDeducted)
            {
                CheckButtonPressesAndDeduct();
                buttonCheckDeducted = true;
            }

            // Notify the main task manager that Task3 is complete.
            taskManagerController.CompleteTask();
            taskTransitionManager3.PostTask3Transition();
        }
        else
        {
            Debug.LogWarning("Mod3Task3 is either already complete or not the active task.");
        }
    }

    /// <summary>
    /// Checks if each required button was pressed.
    /// Deducts 3 points for each missing button.
    /// </summary>
    private void CheckButtonPressesAndDeduct()
    {
        int missingCount = 0;
        if (!image1Pressed) missingCount++;
        if (!bothImagesPressed) missingCount++;
        if (!image2Pressed) missingCount++;
        if (!zoomInPressed) missingCount++;
        if (!zoomOutPressed) missingCount++;
        if (!resetPressed) missingCount++;

        Debug.Log($"Button press check in Task3: Missing {missingCount} required button(s).");

        if (missingCount > 0 && AssessmentController.Instance != null)
        {
            float deduction = missingCount * 3f; // Deduct 3 points for each missing button.
            Debug.Log($"Deducting {deduction} point(s) for incomplete button presses in Task3.");
            AssessmentController.Instance.LogMistake(
                "Task3",
                "Available tools was not used to analyze the bullet.",
                deduction,
                "Make sure to maximize the tools available for analysis."
            );
        }
        else
        {
            Debug.Log("All required buttons in Task3 were pressed. No deduction.");
        }
    }

    // ----- Helper Functions for Panel Layout -----

    /// <summary>
    /// Sets the given panel to full screen.
    /// </summary>
    private void SetFullScreen(GameObject panel)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// Arranges image1Panel and image2Panel in a split-screen layout.
    /// </summary>
    private void SetSplitScreen()
    {
        RectTransform rt1 = image1Panel.GetComponent<RectTransform>();
        if (rt1 != null)
        {
            rt1.anchorMin = new Vector2(0f, 0f);
            rt1.anchorMax = new Vector2(0.5f, 1f);
            rt1.offsetMin = Vector2.zero;
            rt1.offsetMax = Vector2.zero;
        }

        RectTransform rt2 = image2Panel.GetComponent<RectTransform>();
        if (rt2 != null)
        {
            rt2.anchorMin = new Vector2(0.5f, 0f);
            rt2.anchorMax = new Vector2(1f, 1f);
            rt2.offsetMin = Vector2.zero;
            rt2.offsetMax = Vector2.zero;
        }
    }
}
