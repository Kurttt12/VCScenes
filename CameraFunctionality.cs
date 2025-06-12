using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem; // Required for InputActionProperty

public class CameraFunctionality : MonoBehaviour
{
    public AudioClip cameraClickSound;
    public Light cameraFlash;
    public XRGrabInteractable grabInteractable;
    public InputActionProperty triggerAction;
    public XRBaseController leftController;
    public XRBaseController rightController;
    public float hapticDuration = 0.1f;
    public float hapticIntensity = 0.5f;
    public float strongHapticIntensity = 1.0f;
    public System.Collections.Generic.List<Collider> targetColliders;
    public Camera playerCamera;
    public Task1Manager task1Manager;
    public Task2Manager task2Manager;
    public Task3Manager task3Manager;

    private bool isGrabbing = false;
    private bool isTriggerPressed = false;

    void Start()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        if (playerCamera == null)
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("CameraFunctionality script requires a Camera component.");
                enabled = false;
                return;
            }
        }

        if (cameraFlash != null)
        {
            cameraFlash.enabled = false;
        }

        Debug.Log("CameraFunctionality script initialized.");
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    void Update()
    {
        if (isGrabbing && triggerAction.action != null)
        {
            float triggerValue = triggerAction.action.ReadValue<float>();

            if (triggerValue > 0.5f && !isTriggerPressed)
            {
                isTriggerPressed = true;
                Capture();
            }
            else if (triggerValue <= 0.5f)
            {
                isTriggerPressed = false;
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // if this grab came from a socket interactor (i.e. your pocket), ignore it
        if (args.interactorObject is XRSocketInteractor)
            return;

        // otherwise it really is the controller picking it up
        isGrabbing = true;
        Debug.Log("Camera grabbed.");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbing = false;
        Debug.Log("Camera released.");
    }

    public void Capture()
    {
        Debug.Log("Capture button pressed.");

        if (task1Manager != null && task1Manager.taskManagerController.IsCurrentTask(task1Manager.gameObject))
        {
            ProcessCaptureForTask1();
        }
        else
        {
            Collider capturedTarget = GetCapturedTarget();
            if (capturedTarget != null && IsCorrectTaskAndTarget(capturedTarget))
            {
                // For Task3, check if the captured evidence is the current one.
                if (task3Manager != null && task3Manager.taskManagerController.IsCurrentTask(task3Manager.gameObject))
                {
                    Evidence evidence = capturedTarget.GetComponent<Evidence>();
                    Evidence currentEvidence = task3Manager.GetCurrentEvidence();
                    if (evidence != null && currentEvidence != null && evidence != currentEvidence)
                    {
                        Debug.Log("Captured evidence is not the current evidence. No sound or haptic feedback will be applied.");
                        ProcessCapture(capturedTarget);
                        return;
                    }
                }

                if (cameraClickSound != null)
                {
                    AudioSource.PlayClipAtPoint(cameraClickSound, transform.position);
                }

                if (cameraFlash != null)
                {
                    StartCoroutine(CameraFlash());
                }

                if (rightController != null)
                {
                    rightController.SendHapticImpulse(hapticIntensity, hapticDuration);
                }

                ProcessCapture(capturedTarget);
            }
            else
            {
                // If Task2 is active and the victim (dead body) isn't fully in frame, log a deduction using Task2Manager.
                if (task2Manager != null && task2Manager.taskManagerController.IsCurrentTask(task2Manager.gameObject))
                {
                    task2Manager.CaptureFailed();
                }
                // For Task3, check if the issue is that the evidence is not fully in frame.
                if (task3Manager != null && task3Manager.taskManagerController.IsCurrentTask(task3Manager.gameObject))
                {
                    // Look for any Evidence in targetColliders.
                    Evidence evidenceCandidate = null;
                    foreach (Collider col in targetColliders)
                    {
                        if (col.CompareTag("Evidence"))
                        {
                            evidenceCandidate = col.GetComponent<Evidence>();
                            if (evidenceCandidate != null)
                                break;
                        }
                    }
                    if (evidenceCandidate != null && !task3Manager.IsEvidenceInFrame(evidenceCandidate))
                    {
                        Debug.Log("Evidence is not fully in frame for Task3.");
                        SoundNotification.Instance.PlaySound("incorrect");
                        AssessmentController.Instance.LogMistake(
                            "Task3",
                            "Evidence not fully in frame during capture.",
                            5f,
                            "Adjust your view so the entire evidence is visible."
                        );
                    }
                }

                TriggerHapticFeedback(strongHapticIntensity);
                Debug.Log("Incorrect task or target. Capture not processed.");
            }
        }
    }

    private void ProcessCaptureForTask1()
    {
        // Verify that the camera's zoom (field of view) is on the required level.
        float requiredZoom = 50f;
        if (Mathf.Abs(playerCamera.fieldOfView - requiredZoom) > 0.1f)
        {
            Debug.Log("Camera zoom level is not correct for Task1 capture. It must be at 50f.");
            // Log a mistake for Task1.
            SoundNotification.Instance.PlaySound("incorrect");
            AssessmentController.Instance.LogMistake("Task1", "Camera zoom level is not correct.", 5f, "Adjust the camera's FOV to Wide Lens before capturing.");
            TriggerHapticFeedback(strongHapticIntensity);
            return; // Abort capture if zoom is not correct.
        }
        
        int currentIndex = task1Manager.CurrentTaskIndex;
        bool captureProcessed = false;
        
        foreach (var cylinderTrigger in task1Manager.taskInfoArray[currentIndex].cylinderTriggers)
        {
            if (cylinderTrigger.playerInside)
            {
                BoxCollider boxCollider = cylinderTrigger.captureBoxCollider;
                if (boxCollider != null)
                {
                    if (!IsTargetInView(boxCollider))
                    {
                        Debug.Log("Cylinder trigger's capture area (BoxCollider) is not in view.");
                        SoundNotification.Instance.PlaySound("incorrect");
                        AssessmentController.Instance.LogMistake("Task1", "Capture area is not fully in view.", 3f, "Ensure the entire capture area is visible before capturing.");
                        TriggerHapticFeedback(strongHapticIntensity);
                        return; // Abort capture if not in view.
                    }
                }
                else
                {
                    Debug.LogWarning("BoxCollider not assigned on cylinder trigger. Capture validation by camera view may not work as expected.");
                }

                // Process the capture for Task1.
                cylinderTrigger.Capture();

                if (cameraClickSound != null)
                {
                    AudioSource.PlayClipAtPoint(cameraClickSound, transform.position);
                }

                if (cameraFlash != null)
                {
                    StartCoroutine(CameraFlash());
                }

                if (rightController != null)
                {
                    rightController.SendHapticImpulse(hapticIntensity, hapticDuration);
                }

                Debug.Log("Capture processed for Task1Manager.");
                captureProcessed = true;
                break;
            }
        }
        
        if (!captureProcessed)
        {
            Debug.Log("No valid cylinder trigger with player inside and capture area in view was found for capture processing.");
            SoundNotification.Instance.PlaySound("incorrect");
            AssessmentController.Instance.LogMistake("Task1", "Not positioned in the corner of the room for the picture.", 2f, "Make sure you're standing in the corner of the room before taking the picture.");
        }
    }

    private bool IsCorrectTaskAndTarget(Collider capturedTarget)
    {
        if (task2Manager != null && task2Manager.taskManagerController.IsCurrentTask(task2Manager.gameObject))
        {
            return capturedTarget.gameObject == task2Manager.victimObject;
        }

        if (task3Manager != null && task3Manager.taskManagerController.IsCurrentTask(task3Manager.gameObject))
        {
            // For Task3, any collider that has an Evidence component is considered valid.
            Evidence evidence = capturedTarget.GetComponent<Evidence>();
            return (evidence != null);
        }
        return false;
    }

    private void ProcessCapture(Collider capturedTarget)
    {
        if (task2Manager != null && task2Manager.taskManagerController.IsCurrentTask(task2Manager.gameObject))
        {
            task2Manager.OnVictimCaptured();
        }
        else if (task3Manager != null && task3Manager.taskManagerController.IsCurrentTask(task3Manager.gameObject))
        {
            Evidence evidence = capturedTarget.GetComponent<Evidence>();
            if (evidence != null)
            {
                if (!evidence.isCaptured)
                {
                    task3Manager.OnEvidenceCaptured(capturedTarget.gameObject);
                }
                else if (evidence.finalCaptureDetector != null && !evidence.finalCaptureDetector.isFinalCaptured)
                {
                    task3Manager.OnFinalCaptureCaptured(capturedTarget.gameObject);
                }
            }
        }
    }

    private Collider GetCapturedTarget()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        foreach (Collider targetCollider in targetColliders)
        {
            if (targetCollider.CompareTag("Evidence") && IsTargetInView(targetCollider) && HasClearLineOfSight(targetCollider))
            {
                return targetCollider;
            }
        }
        return null;
    }

    private bool IsTargetInView(Collider targetCollider)
    {
        Bounds bounds = targetCollider.bounds;

        Vector3[] corners = new Vector3[8];
        corners[0] = bounds.min;
        corners[1] = bounds.max;
        corners[2] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[3] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[4] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        corners[5] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[6] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        corners[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        foreach (Vector3 corner in corners)
        {
            if (!GeometryUtility.TestPlanesAABB(planes, new Bounds(corner, Vector3.zero)))
            {
                return false;
            }
        }
        return true;
    }

    private bool HasClearLineOfSight(Collider targetCollider)
    {
        Bounds bounds = targetCollider.bounds;
        Vector3[] points = new Vector3[]
        {
            bounds.center,
            bounds.min,
            bounds.max,
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z)
        };

        foreach (Vector3 point in points)
        {
            Vector3 direction = (point - playerCamera.transform.position).normalized;
            float distance = Vector3.Distance(playerCamera.transform.position, point);

            if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, distance))
            {
                if (hit.collider != targetCollider && (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Floor")))
                {
                    Debug.Log($"Blocked by: {hit.collider.gameObject.name}");
                    return false;
                }
            }
        }
        return true;
    }

    private void TriggerHapticFeedback(float intensity)
    {
        if (leftController != null)
        {
            leftController.SendHapticImpulse(intensity, hapticDuration);
        }

        if (rightController != null)
        {
            rightController.SendHapticImpulse(intensity, hapticDuration);
        }
    }

    private System.Collections.IEnumerator CameraFlash()
    {
        cameraFlash.enabled = true;
        yield return new WaitForSeconds(0.1f);
        cameraFlash.enabled = false;
    }
}
