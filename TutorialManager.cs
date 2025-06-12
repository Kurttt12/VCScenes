using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialManager : MonoBehaviour
{
    public GameObject welcomeImage;         // Assign the Welcome Image
    public GameObject tutorialNextButton;   // Assign the Tutorial Next button
    public TextMeshProUGUI[] stepTexts;     // Assign the TextMeshPro UIs for each step
    public GameObject[] stepImages;         // Assign the images for each step
    public GameObject objectToGrab;         // Assign the object to grab (e.g., Sphere)
    public TutorialTrigger targetTrigger;       // Assign the TutorialTrigger component
    public TextMeshProUGUI congratulationsText; // Assign the "Congratulations" UI Text
    public Button congratulationsButton;    // Assign the Congratulations button
    public GameObject newGameObject;        // The new GameObject to open

    private bool isTutorialStarted = false; // Tracks if the tutorial has started
    private bool hasLookedAround = false; 
    private bool hasEnteredTargetArea = false; 
    private bool hasGrabbedObject = false;
    private bool step2Completed = false; // Tracks if Step 2 is completed

    // Initial rotation and position of the camera
    private float initialYaw;
    private Vector3 initialPosition;

    private void Start()
    {
        // Ensure Rigidbody component is set up
        Rigidbody rb = targetTrigger.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = targetTrigger.gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        // Show the welcome image and button, hide everything else
        welcomeImage.SetActive(true);
        tutorialNextButton.SetActive(true);
        HideAllSteps();
        congratulationsText.gameObject.SetActive(false);
        congratulationsButton.gameObject.SetActive(false); // Hide the button initially
        objectToGrab.SetActive(false); // Hide the object initially
        targetTrigger.gameObject.SetActive(false); // Hide target area initially
        newGameObject.SetActive(false); // Hide the new GameObject initially

        // Hook the button events
        tutorialNextButton.GetComponent<Button>().onClick.AddListener(StartTutorial);
        congratulationsButton.onClick.AddListener(OpenNewGameObject);

        // Record initial yaw rotation and position
        initialYaw = Camera.main.transform.eulerAngles.y;
        initialPosition = Camera.main.transform.position;
    }

    public void StartTutorial()
    {
        // Triggered by the button click to start the tutorial
        welcomeImage.SetActive(false);
        tutorialNextButton.SetActive(false);
        ShowStep(0);
        isTutorialStarted = true;
        Debug.Log("Tutorial started: Step 1 initiated.");
    }

    private void Update()
    {
        // Only proceed if the tutorial has started
        if (!isTutorialStarted) return;

        // Step 1: Check if the user has looked around
        if (!hasLookedAround && CheckHeadMovement())
        {
            hasLookedAround = true;
            ShowStep(1);
            targetTrigger.gameObject.SetActive(true); // Enable the target area
            Debug.Log("Step 1 completed: User has looked around.");
        }

        // Step 2: Check if the player has entered the target area
        if (hasLookedAround && hasEnteredTargetArea && !step2Completed)
        {
            step2Completed = true; // Mark Step 2 as completed
            ShowStep(2);
            objectToGrab.SetActive(true); // Make the object visible for grabbing
            Debug.Log("Step 2 completed: User has entered the target area.");
        }

        // Step 3: Check if the user has grabbed the object
        if (hasEnteredTargetArea && !hasGrabbedObject && CheckObjectGrabbed())
        {
            hasGrabbedObject = true;
            CompleteTutorial();
            Debug.Log("Step 3 completed: User has grabbed the object.");
        }
    }

    public void TriggerEntered(TutorialTrigger trigger)
    {
        if (trigger == targetTrigger)
        {
            hasEnteredTargetArea = true;
            trigger.MarkAsTriggered(); // Mark the trigger as completed
            Debug.Log("TriggerEntered: Target area entered.");
        }
    }

    private void ShowStep(int stepIndex)
    {
        HideAllSteps();
        if (stepIndex < stepTexts.Length)
        {
            stepTexts[stepIndex].gameObject.SetActive(true);
            switch(stepIndex)
            {
                case 0:
                    stepTexts[stepIndex].text = "<b>Hello!</b> \nMake sure your VR headset fits properly, and move your head around to experience the immersive simulation.";
                    break;
                case 1:
                    stepTexts[stepIndex].text = "<b>Great job!</b>\nNow, you can use the joystick on your left controller for player controls and the joystick on your right controller to control your point of view.\n\n<b><i>Proceed to the glowing area to continue with the tutorial</i></b>";
                    break;
                case 2:
                    stepTexts[stepIndex].text = "<b>Great!</b> To grab an object, use the hand trigger on either the left or right controllers, which is placed in your middle finger.";
                    break;
            }
        }
        if (stepIndex < stepImages.Length)
        {
            stepImages[stepIndex].SetActive(true);
        }
    }

    private void HideAllSteps()
    {
        foreach (var text in stepTexts)
        {
            text.gameObject.SetActive(false);
        }
        foreach (var image in stepImages)
        {
            image.SetActive(false);
        }
    }

    private bool CheckHeadMovement()
    {
        // Check if the user has looked in multiple directions
        float currentYaw = Camera.main.transform.eulerAngles.y;
        float yawDifference = Mathf.Abs(Mathf.DeltaAngle(initialYaw, currentYaw));
        bool hasMovedHead = yawDifference > 45; // Adjust angle if needed
        Debug.Log($"Checking head movement: {hasMovedHead}, Yaw Difference: {yawDifference}");
        return hasMovedHead;
    }

    private bool CheckObjectGrabbed()
    {
        // Check if the object has been grabbed using XR Interaction events
        XRGrabInteractable grabInteractable = objectToGrab.GetComponent<XRGrabInteractable>();
        bool isGrabbed = grabInteractable != null && grabInteractable.isSelected;
        Debug.Log($"Checking object grabbed: {isGrabbed}");
        return isGrabbed;
    }

    private void CompleteTutorial()
    {
        HideAllSteps(); // Hide all step-related UI elements
        congratulationsText.text = "Congratulations! You've completed the tutorial.";
        congratulationsText.gameObject.SetActive(true); // Show the congratulations message
        congratulationsButton.gameObject.SetActive(true); // Show the button to open new GameObject
        Debug.Log("Tutorial completed: Congratulations message displayed.");
    }

    private void OpenNewGameObject()
    {
        newGameObject.SetActive(true); // Show the new GameObject
        congratulationsText.gameObject.SetActive(false); // Hide the congratulations message
        congratulationsButton.gameObject.SetActive(false); // Hide the congratulations button
    }
}
