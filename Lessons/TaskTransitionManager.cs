using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class TaskTransitionManager : MonoBehaviour
{
    #region NPC and Teleport Settings
    [Header("NPC References")]
    [Tooltip("NPC used for the Introduction dialogue.")]
    public GameObject introductionNPC;
    [Tooltip("NPC used for Task 1 instructions (and also for the Camera Transition stage).")]
    public GameObject task1NPC;
    [Tooltip("NPC used for Task 2 instructions.")]
    public GameObject task2NPC;
    [Tooltip("NPC used for Task 3 instructions.")]
    public GameObject task3NPC;
    [Tooltip("NPC used for the Ending dialogue.")]
    public GameObject endingNPC;

    [Header("Teleport Locations")]
    [Tooltip("Teleport target for the Introduction (player will be teleported there immediately).")]
    public Transform introductionTeleport;
    [Tooltip("Teleport target for Task 1.")]
    public Transform task1Teleport;
    [Tooltip("Teleport target for Task 2.")]
    public Transform task2Teleport;
    [Tooltip("Teleport target for Task 3.")]
    public Transform task3Teleport;
    [Tooltip("Teleport target for the Ending.")]
    public Transform endingTeleport;
    #endregion

    #region Player and Movement Settings
    [Header("Player & Movement")]
    [Tooltip("Reference to the player (XR rig) GameObject.")]
    public GameObject player;
    [Tooltip("Reference to the XR movement provider (e.g., ActionBasedContinuousMoveProvider).")]
    public ActionBasedContinuousMoveProvider continuousMoveProvider;
    #endregion

    #region Miscellaneous Components
    [Header("Tool Reattachment")]
    [Tooltip("Reference to the ReattachToolsToPlayer component for reattaching tools.")]
    public ReattachToolsToPlayer toolReattacher;
    #endregion

    #region Camera Transition Settings
    [Header("Camera Transition Settings")]
    [Tooltip("Alternate audio clip for the Camera Transition stage (using the same NPC as Task 1).")]
    public AudioClip cameraTransitionAudioClip;
    [Tooltip("The barrier object that should be hidden after the Camera Transition stage.")]
    public GameObject barrierObject;
    [Tooltip("Reference to the camera tool. Its L_CameraFunctionality component is used to check if it's grabbed.")]
    public GameObject cameraTool;
    #endregion

    #region Timing Settings
    [Header("Timing Settings")]
    [Tooltip("Fallback duration (in seconds) if no audio clip is present on an NPC.")]
    public float fallbackDialogueDuration = 5f;
    [Tooltip("Additional delay after an NPC's audio finishes, before transitioning.")]
    public float delayAfterAudio = 2f;
    #endregion

    #region Post-Transition Settings
    [Header("Post-Capture Settings")]
    [Tooltip("Alternate audio clip for Task 1's post-capture dialogue.")]
    public AudioClip postCaptureAudioClip;

    [Header("Post-Task2 Settings")]
    [Tooltip("Alternate audio clip for Task 2's post-transition dialogue.")]
    public AudioClip postTask2AudioClip;

    [Header("Post-Task3 Settings")]
    [Tooltip("Alternate audio clip for Task 3's post-transition dialogue.")]
    public AudioClip postTask3AudioClip;
    #endregion

    #region Ending Transition Settings
    [Header("Ending Transition Settings")]
    [Tooltip("Object to show during the Ending transition.")]
    public GameObject endingObject;
    [Tooltip("Left hand ray interactor to show during the Ending transition.")]
    public GameObject leftHandRayInteractor;
    [Tooltip("Right hand ray interactor to show during the Ending transition.")]
    public GameObject rightHandRayInteractor;
    #endregion

    #region Helper Functions
    private void HideAllNPCs()
    {
        if (introductionNPC != null) introductionNPC.SetActive(false);
        if (task1NPC != null) task1NPC.SetActive(false);
        if (task2NPC != null) task2NPC.SetActive(false);
        if (task3NPC != null) task3NPC.SetActive(false);
        if (endingNPC != null) endingNPC.SetActive(false);
    }

    private void ShowNPC(GameObject npc)
    {
        HideAllNPCs();
        if (npc != null)
            npc.SetActive(true);
    }
    #endregion

    void Awake()
    {
        Debug.Log("[TaskTransitionManager] Awake called.");
        // Hide all NPCs by default.
        HideAllNPCs();
    }

    void Start()
    {
        Debug.Log("[TaskTransitionManager] Start called.");
        // For testing purposes, automatically trigger the Introduction transition.
        StartIntroductionTransition();
    }

    #region Standard Transition Methods

    public void StartIntroductionTransition()
    {
        Debug.Log("[TaskTransitionManager] StartIntroductionTransition() called.");
        StartCoroutine(TransitionRoutine(introductionNPC, introductionTeleport));
    }

    public void StartTask1Transition()
    {
        Debug.Log("[TaskTransitionManager] StartTask1Transition() called.");
        if (toolReattacher != null)
        {
            toolReattacher.ReattachTools();
            Debug.Log("[TaskTransitionManager] Tools reattached via ReattachToolsToPlayer.");
        }
        else
        {
            Debug.LogWarning("[TaskTransitionManager] ReattachToolsToPlayer reference is missing!");
        }
        StartCoroutine(TransitionRoutine(task1NPC, task1Teleport));
    }

    public void StartTask2Transition()
    {
        Debug.Log("[TaskTransitionManager] StartTask2Transition() called.");
        StartCoroutine(TransitionRoutine(task2NPC, task2Teleport));
    }

    public void StartTask3Transition()
    {
        Debug.Log("[TaskTransitionManager] StartTask3Transition() called.");
        StartCoroutine(TransitionRoutine(task3NPC, task3Teleport));
    }

    /// <summary>
    /// In addition to triggering the Ending dialogue through the endingNPC,
    /// this method also ensures that the ending object and both left/right hand ray interactors are activated.
    /// </summary>
    public void StartEndingTransition()
    {
        Debug.Log("[TaskTransitionManager] StartEndingTransition() called.");
        // Activate the designated ending object and both ray interactors.
        if (endingObject != null)
        {
            endingObject.SetActive(true);
            Debug.Log("[TaskTransitionManager] Ending object activated.");
        }
        else
        {
            Debug.LogWarning("[TaskTransitionManager] Ending object is not assigned!");
        }
        if (leftHandRayInteractor != null)
        {
            leftHandRayInteractor.SetActive(true);
            Debug.Log("[TaskTransitionManager] Left hand ray interactor activated.");
        }
        else
        {
            Debug.LogWarning("[TaskTransitionManager] Left hand ray interactor is not assigned!");
        }
        if (rightHandRayInteractor != null)
        {
            rightHandRayInteractor.SetActive(true);
            Debug.Log("[TaskTransitionManager] Right hand ray interactor activated.");
        }
        else
        {
            Debug.LogWarning("[TaskTransitionManager] Right hand ray interactor is not assigned!");
        }
        StartCoroutine(TransitionRoutine(endingNPC, endingTeleport));
    }

    #endregion

    #region Transition Routine

    private IEnumerator TransitionRoutine(GameObject npc, Transform teleportTarget)
    {
        Debug.Log("[TransitionRoutine] Started for NPC: " + (npc != null ? npc.name : "null"));

        // Disable player movement.
        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enabled = false;
            Debug.Log("[TransitionRoutine] Player movement disabled.");
        }
        else
        {
            Debug.LogWarning("[TransitionRoutine] ContinuousMoveProvider reference is missing!");
        }

        // Teleport the player.
        if (teleportTarget != null && player != null)
        {
            player.transform.position = teleportTarget.position;
            player.transform.rotation = teleportTarget.rotation;
            Debug.Log("[TransitionRoutine] Player teleported to: " + teleportTarget.name);
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            Debug.LogWarning("[TransitionRoutine] Teleport target or player reference is missing!");
        }

        // Hide all NPCs then show the desired one.
        ShowNPC(npc);
        Debug.Log("[TransitionRoutine] Showing NPC: " + npc.name);

        // Trigger the talking animation.
        Animator npcAnimator = npc.GetComponent<Animator>();
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger("talking");
            Debug.Log("[TransitionRoutine] 'talking' trigger set on " + npc.name);
        }
        else
        {
            Debug.LogWarning("[TransitionRoutine] No Animator found on " + npc.name);
        }
        if (npc == introductionNPC)
        {
            Debug.Log("[TransitionRoutine] Waiting an extra 1 second for Introduction NPC...");
            yield return new WaitForSeconds(1f);
        }

        // Play the NPC's dialogue.
        AudioSource npcAudio = npc.GetComponent<AudioSource>();
        if (npcAudio != null && npcAudio.clip != null)
        {
            if (!npcAudio.isPlaying)
            {
                npcAudio.Play();
                Debug.Log("[TransitionRoutine] Playing audio clip: " + npcAudio.clip.name + " on " + npc.name);
            }
            yield return new WaitWhile(() => npcAudio.isPlaying);
            Debug.Log("[TransitionRoutine] Audio finished on " + npc.name);
        }
        else
        {
            Debug.LogWarning("[TransitionRoutine] No AudioSource or audio clip on " + npc.name + ". Using fallback duration: " + fallbackDialogueDuration + " seconds.");
            yield return new WaitForSeconds(fallbackDialogueDuration);
        }

        yield return new WaitForSeconds(delayAfterAudio);

        // Trigger the breathing (idle) animation.
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger("breathing");
            Debug.Log("[TransitionRoutine] 'breathing' trigger set on " + npc.name);
        }
        else
        {
            Debug.LogWarning("[TransitionRoutine] No Animator found on " + npc.name + " to trigger breathing.");
        }

        // Chain based on which NPC is active.
        if (npc == introductionNPC)
        {
            Debug.Log("[TransitionRoutine] Introduction complete. Automatically chaining to Task 1 transition.");
            StartTask1Transition();
            yield break;
        }
        else if (npc == task1NPC)
        {
            Debug.Log("[TransitionRoutine] Waiting indefinitely for the camera to be grabbed...");
            yield return new WaitUntil(() =>
            {
                if (cameraTool != null)
                {
                    var camFunc = cameraTool.GetComponent<L_CameraFunctionality>();
                    if (camFunc != null)
                        return camFunc.isGrabbing;
                    else
                    {
                        Debug.LogWarning("[TransitionRoutine] L_CameraFunctionality component missing on cameraTool.");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning("[TransitionRoutine] cameraTool reference is missing.");
                    return false;
                }
            });
            Debug.Log("[TransitionRoutine] Camera grabbed. Initiating Camera Transition stage.");
            yield return StartCoroutine(CameraTransitionRoutine());
            if (continuousMoveProvider != null)
            {
                continuousMoveProvider.enabled = true;
                Debug.Log("[TransitionRoutine] Player movement re-enabled after Camera Transition.");
            }
            else
            {
                Debug.LogWarning("[TransitionRoutine] ContinuousMoveProvider reference is null! Check if it is assigned.");
            }
            yield break;
        }
        else
        {
            if (continuousMoveProvider != null)
            {
                continuousMoveProvider.enabled = true;
                Debug.Log("[TransitionRoutine] Player movement re-enabled.");
            }
            else
            {
                Debug.LogWarning("[TransitionRoutine] ContinuousMoveProvider reference is null! Check if it is assigned.");
            }
            // End routine and let external logic decide the next step.
        }

        yield break;
    }
    #endregion

    #region Camera Transition Routine

    public void StartCameraTransition()
    {
        Debug.Log("[TaskTransitionManager] StartCameraTransition() called.");
        StartCoroutine(CameraTransitionRoutine());
    }

    private IEnumerator CameraTransitionRoutine()
    {
        GameObject npc = task1NPC;
        if (npc == null)
        {
            Debug.LogWarning("[CameraTransitionRoutine] Task 1 NPC reference is missing!");
            yield break;
        }
        ShowNPC(npc);
        Debug.Log("[CameraTransitionRoutine] Using Task 1 NPC for Camera Transition.");
        Animator npcAnimator = npc.GetComponent<Animator>();
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger("talking");
            Debug.Log("[CameraTransitionRoutine] 'talking' trigger set on " + npc.name);
        }
        else
        {
            Debug.LogWarning("[CameraTransitionRoutine] No Animator found on " + npc.name);
        }
        yield return new WaitForSeconds(1f);
        AudioSource npcAudio = npc.GetComponent<AudioSource>();
        if (npcAudio != null && cameraTransitionAudioClip != null)
        {
            npcAudio.clip = cameraTransitionAudioClip;
            npcAudio.Play();
            Debug.Log("[CameraTransitionRoutine] Playing camera transition audio clip: " + cameraTransitionAudioClip.name + " on " + npc.name);
            yield return new WaitWhile(() => npcAudio.isPlaying);
            Debug.Log("[CameraTransitionRoutine] Camera transition audio finished on " + npc.name);
        }
        else
        {
            Debug.LogWarning("[CameraTransitionRoutine] Either AudioSource on " + npc.name + " or cameraTransitionAudioClip is missing. Using fallback.");
            yield return new WaitForSeconds(fallbackDialogueDuration);
        }
        yield return new WaitForSeconds(delayAfterAudio);
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger("breathing");
            Debug.Log("[CameraTransitionRoutine] 'breathing' trigger set on " + npc.name);
        }
        if (barrierObject != null)
        {
            barrierObject.SetActive(false);
            Debug.Log("[CameraTransitionRoutine] Barrier object hidden.");
        }
        else
        {
            Debug.LogWarning("[CameraTransitionRoutine] Barrier object reference is missing!");
        }
        Debug.Log("[CameraTransitionRoutine] Camera Transition complete. External logic will now decide when to proceed.");
        yield break;
    }
    #endregion

    #region Post-Capture Transition

    public void PostCameraTask1()
    {
        Debug.Log("[TaskTransitionManager] PostCameraTask1() called.");
        StartCoroutine(PostCameraTask1Routine());
    }

    private IEnumerator PostCameraTask1Routine()
    {
        if (task1Teleport != null && player != null)
        {
            player.transform.position = task1Teleport.position;
            player.transform.rotation = task1Teleport.rotation;
            Debug.Log("[PostCameraTask1Routine] Player teleported to Task 1 position.");
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            Debug.LogWarning("[PostCameraTask1Routine] Task 1 teleport target or player reference is missing!");
        }
        ShowNPC(task1NPC);
        Debug.Log("[PostCameraTask1Routine] Task 1 NPC activated for post-capture dialogue.");
        AudioSource npcAudio = task1NPC.GetComponent<AudioSource>();
        if (npcAudio != null && postCaptureAudioClip != null)
        {
            npcAudio.clip = postCaptureAudioClip;
            npcAudio.Play();
            Debug.Log("[PostCameraTask1Routine] Playing post-capture audio: " + postCaptureAudioClip.name + " on " + task1NPC.name);
            yield return new WaitWhile(() => npcAudio.isPlaying);
            Debug.Log("[PostCameraTask1Routine] Post-capture audio finished on " + task1NPC.name);
        }
        else
        {
            Debug.LogWarning("[PostCameraTask1Routine] Missing AudioSource on " + task1NPC.name + " or postCaptureAudioClip not assigned. Using fallback.");
            yield return new WaitForSeconds(fallbackDialogueDuration);
        }
        task1NPC.SetActive(false);
        Debug.Log("[PostCameraTask1Routine] Task 1 NPC hidden after post-capture dialogue.");
        yield return new WaitForSeconds(delayAfterAudio);
        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enabled = true;
            Debug.Log("[PostCameraTask1Routine] Player movement re-enabled.");
        }
        else
        {
            Debug.LogWarning("[PostCameraTask1Routine] ContinuousMoveProvider reference is null! Check if it is assigned.");
        }
        Debug.Log("[PostCameraTask1Routine] Post-capture sequence complete. Automatically chaining to Task 2 transition.");
        StartTask2Transition();
        yield break;
    }
    #endregion

    #region Post-Task2 Transition

    public void PostTask2Transition()
    {
        Debug.Log("[TaskTransitionManager] PostTask2Transition() called.");
        StartCoroutine(PostTask2TransitionRoutine());
    }

    private IEnumerator PostTask2TransitionRoutine()
    {
        if (task2Teleport != null && player != null)
        {
            player.transform.position = task2Teleport.position;
            player.transform.rotation = task2Teleport.rotation;
            Debug.Log("[PostTask2TransitionRoutine] Player teleported to Task 2 position.");
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            Debug.LogWarning("[PostTask2TransitionRoutine] Task 2 teleport target or player reference is missing!");
        }
        ShowNPC(task2NPC);
        Debug.Log("[PostTask2TransitionRoutine] Task 2 NPC activated for post-task2 dialogue.");
        AudioSource npcAudio = task2NPC.GetComponent<AudioSource>();
        if (npcAudio != null && postTask2AudioClip != null)
        {
            npcAudio.clip = postTask2AudioClip;
            npcAudio.Play();
            Debug.Log("[PostTask2TransitionRoutine] Playing post-task2 audio: " + postTask2AudioClip.name + " on " + task2NPC.name);
            yield return new WaitWhile(() => npcAudio.isPlaying);
            Debug.Log("[PostTask2TransitionRoutine] Post-task2 audio finished on " + task2NPC.name);
        }
        else
        {
            Debug.LogWarning("[PostTask2TransitionRoutine] Missing AudioSource on " + task2NPC.name + " or postTask2AudioClip not assigned. Using fallback.");
            yield return new WaitForSeconds(fallbackDialogueDuration);
        }
        task2NPC.SetActive(false);
        Debug.Log("[PostTask2TransitionRoutine] Task 2 NPC hidden after post-task2 dialogue.");
        yield return new WaitForSeconds(delayAfterAudio);
        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enabled = true;
            Debug.Log("[PostTask2TransitionRoutine] Player movement re-enabled.");
        }
        else
        {
            Debug.LogWarning("[PostTask2TransitionRoutine] ContinuousMoveProvider reference is null! Check if it is assigned.");
        }
        Debug.Log("[PostTask2TransitionRoutine] Post-task2 sequence complete. Automatically chaining to Task 3 transition.");
        StartTask3Transition();
        yield break;
    }
    #endregion

    #region Post-Task3 Transition

    public void PostTask3Transition()
    {
        Debug.Log("[TaskTransitionManager] PostTask3Transition() called.");
        StartCoroutine(PostTask3TransitionRoutine());
    }

    private IEnumerator PostTask3TransitionRoutine()
    {
        if (task3Teleport != null && player != null)
        {
            player.transform.position = task3Teleport.position;
            player.transform.rotation = task3Teleport.rotation;
            Debug.Log("[PostTask3TransitionRoutine] Player teleported to Task 3 position.");
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            Debug.LogWarning("[PostTask3TransitionRoutine] Task 3 teleport target or player reference is missing!");
        }
        ShowNPC(task3NPC);
        Debug.Log("[PostTask3TransitionRoutine] Task 3 NPC activated for post-task3 dialogue.");
        AudioSource npcAudio = task3NPC.GetComponent<AudioSource>();
        if (npcAudio != null && postTask3AudioClip != null)
        {
            npcAudio.clip = postTask3AudioClip;
            npcAudio.Play();
            Debug.Log("[PostTask3TransitionRoutine] Playing post-task3 audio: " + postTask3AudioClip.name + " on " + task3NPC.name);
            yield return new WaitWhile(() => npcAudio.isPlaying);
            Debug.Log("[PostTask3TransitionRoutine] Post-task3 audio finished on " + task3NPC.name);
        }
        else
        {
            Debug.LogWarning("[PostTask3TransitionRoutine] Missing AudioSource on " + task3NPC.name + " or postTask3AudioClip not assigned. Using fallback.");
            yield return new WaitForSeconds(fallbackDialogueDuration);
        }
        task3NPC.SetActive(false);
        Debug.Log("[PostTask3TransitionRoutine] Task 3 NPC hidden after post-task3 dialogue.");
        yield return new WaitForSeconds(delayAfterAudio);
        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enabled = true;
            Debug.Log("[PostTask3TransitionRoutine] Player movement re-enabled.");
        }
        else
        {
            Debug.LogWarning("[PostTask3TransitionRoutine] ContinuousMoveProvider reference is null! Check if it is assigned.");
        }
        Debug.Log("[PostTask3TransitionRoutine] Post-task3 sequence complete. Automatically chaining to Ending transition.");
        StartEndingTransition();
        yield break;
    }
    #endregion
}
