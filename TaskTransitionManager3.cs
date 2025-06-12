using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class TaskTransitionManager3 : MonoBehaviour
{
    #region NPC and Teleport Settings
    [Header("NPC References")]
    [Tooltip("NPC used for the Introduction dialogue.")]
    public GameObject introductionNPC;
    [Tooltip("NPC used for Task 1 instructions.")]
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
        Debug.Log("[TaskTransitionManager3] Awake called.");
        // Hide all NPCs by default.
        HideAllNPCs();
    }

    void Start()
    {
        Debug.Log("[TaskTransitionManager3] Start called.");
        // For testing purposes, automatically trigger the Introduction transition.
        StartIntroductionTransition();
    }

    #region Standard Transition Methods

    public void StartIntroductionTransition()
    {
        Debug.Log("[TaskTransitionManager3] StartIntroductionTransition() called.");
        StartCoroutine(TransitionRoutine(introductionNPC, introductionTeleport));
    }

    public void StartTask1Transition()
    {
        Debug.Log("[TaskTransitionManager3] StartTask1Transition() called.");
        // Removed tool reattachment and camera transition functionality.
        StartCoroutine(TransitionRoutine(task1NPC, task1Teleport));
    }

    public void StartTask2Transition()
    {
        Debug.Log("[TaskTransitionManager3] StartTask2Transition() called.");
        StartCoroutine(TransitionRoutine(task2NPC, task2Teleport));
    }

    public void StartTask3Transition()
    {
        Debug.Log("[TaskTransitionManager3] StartTask3Transition() called.");
        StartCoroutine(TransitionRoutine(task3NPC, task3Teleport));
    }

    /// <summary>
    /// For the Ending transition, in addition to the regular transition, the designated ending object and both left/right hand ray interactors are activated.
    /// </summary>
    public void StartEndingTransition()
    {
        Debug.Log("[TaskTransitionManager3] StartEndingTransition() called.");
        if (endingObject != null)
        {
            endingObject.SetActive(true);
            Debug.Log("[TaskTransitionManager3] Ending object activated.");
        }
        else
        {
            Debug.LogWarning("[TaskTransitionManager3] Ending object is not assigned!");
        }
        if (leftHandRayInteractor != null)
        {
            leftHandRayInteractor.gameObject.SetActive(true);
            Debug.Log("[TaskTransitionManager3] Left hand ray interactor activated.");
        }
        else
        {
            Debug.LogWarning("[TaskTransitionManager3] Left hand ray interactor is not assigned!");
        }
        if (rightHandRayInteractor != null)
        {
            rightHandRayInteractor.gameObject.SetActive(true);
            Debug.Log("[TaskTransitionManager3] Right hand ray interactor activated.");
        }
        else
        {
            Debug.LogWarning("[TaskTransitionManager3] Right hand ray interactor is not assigned!");
        }
        StartCoroutine(TransitionRoutine(endingNPC, endingTeleport));
    }

    #endregion

    #region Transition Routine

    /// <summary>
    /// A common routine that:
    /// 1. Disables player movement and teleports the player.
    /// 2. Hides all NPCs, then shows the specified NPC and triggers its talking animation.
    /// 3. Plays its dialogue (or uses a fallback if missing).
    /// 4. Waits for an extra delay then triggers its breathing (idle) animation.
    /// 5. Finally, if the NPC is the Introduction NPC, it automatically chains to Task 1;
    ///    otherwise, it simply re-enables player movement.
    /// External logic may then decide the next step.
    /// </summary>
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

        // Hide all NPCs and show the current one.
        ShowNPC(npc);
        Debug.Log("[TransitionRoutine] Showing NPC: " + npc.name);

        // Trigger talking animation.
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
        // Additional wait for Introduction NPC.
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

        // Trigger breathing (idle) animation.
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger("breathing");
            Debug.Log("[TransitionRoutine] 'breathing' trigger set on " + npc.name);
        }
        else
        {
            Debug.LogWarning("[TransitionRoutine] No Animator found on " + npc.name + " to trigger breathing.");
        }

        // Chain based on the current NPC.
        if (npc == introductionNPC)
        {
            Debug.Log("[TransitionRoutine] Introduction complete. Automatically chaining to Task 1 transition.");
            StartTask1Transition();
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
            // End routine and allow external logic to decide the next step.
        }
        yield break;
    }
    #endregion

    #region Post-Transition Routines

    // Post-Capture Transition (for Task 1):
    public void PostTask1Transition()
    {
        Debug.Log("[TaskTransitionManager3] PostTask1Transition() called.");
        StartCoroutine(PostTask1TransitionRoutine());
    }

    private IEnumerator PostTask1TransitionRoutine()
    {
        // Teleport back to Task 1.
        if (task1Teleport != null && player != null)
        {
            player.transform.position = task1Teleport.position;
            player.transform.rotation = task1Teleport.rotation;
            Debug.Log("[PostTask1TransitionRoutine] Player teleported to Task 1 position.");
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            Debug.LogWarning("[PostTask1TransitionRoutine] Task 1 teleport target or player reference is missing!");
        }
        // Show Task 1 NPC and play post-capture dialogue.
        ShowNPC(task1NPC);
        Debug.Log("[PostTask1TransitionRoutine] Task 1 NPC activated for post-capture dialogue.");
        AudioSource npcAudio = task1NPC.GetComponent<AudioSource>();
        if (npcAudio != null && postCaptureAudioClip != null)
        {
            npcAudio.clip = postCaptureAudioClip;
            npcAudio.Play();
            Debug.Log("[PostTask1TransitionRoutine] Playing post-capture audio: " + postCaptureAudioClip.name + " on " + task1NPC.name);
            yield return new WaitWhile(() => npcAudio.isPlaying);
            Debug.Log("[PostTask1TransitionRoutine] Post-capture audio finished on " + task1NPC.name);
        }
        else
        {
            Debug.LogWarning("[PostTask1TransitionRoutine] Missing AudioSource on " + task1NPC.name + " or postCaptureAudioClip not assigned. Using fallback.");
            yield return new WaitForSeconds(fallbackDialogueDuration);
        }
        // Hide the Task 1 NPC.
        task1NPC.SetActive(false);
        Debug.Log("[PostTask1TransitionRoutine] Task 1 NPC hidden after post-capture dialogue.");
        yield return new WaitForSeconds(delayAfterAudio);
        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enabled = true;
            Debug.Log("[PostTask1TransitionRoutine] Player movement re-enabled.");
        }
        else
        {
            Debug.LogWarning("[PostTask1TransitionRoutine] ContinuousMoveProvider reference is null! Check if it is assigned.");
        }
        Debug.Log("[PostTask1TransitionRoutine] Post-capture sequence complete. Automatically chaining to Task 2 transition.");
        StartTask2Transition();
        yield break;
    }

    // Post-Task2 Transition:
    public void PostTask2Transition()
    {
        Debug.Log("[TaskTransitionManager3] PostTask2Transition() called.");
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

    // Post-Task3 Transition:
    public void PostTask3Transition()
    {
        Debug.Log("[TaskTransitionManager3] PostTask3Transition() called.");
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
