using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public PlayableDirector playableDirector;
    public int sceneIndex;
    public Vector3 spawnPosition;
    public bool useCutscene = false;

    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    // ✅ New fields for enabling/disabling components
    public MonoBehaviour[] componentsToEnable;
    public GameObject[] objectsToDisableAfterCutscene;

    private static bool isReturningFromCutscene = false;
    
    // ✅ Skip cutscene functionality - Simplified
    [Header("Cutscene Skip")]
    public InputActionProperty skipActivationAction; // Reference to the skip action
    public string mainMenuSceneName = "Main Scene";
    
    // ✅ XR Rig reference for enabling during skip panel
    [Header("XR Rig Settings")]
    public GameObject xrOrigin;
    
    private bool skipRequested = false;

    private void Start()
    {
        if (useCutscene && playableDirector != null)
        {
            playableDirector.stopped += OnCutsceneEnd;
        }

        // ✅ Check if returning from cutscene
        if (isReturningFromCutscene)
        {
            PositionPlayerAtSpawn();
            ManageGameObjects();
            ManageComponents();
            isReturningFromCutscene = false; // Reset flag
        }
    }
    
    private void Update()
    {
        // ✅ Check for controller button press to skip cutscene directly
        if (useCutscene && playableDirector != null && playableDirector.state == PlayState.Playing)
        {
            if (CheckIfButtonPressed() && !skipRequested)
            {
                skipRequested = true;
                SkipCutscene();
            }
        }
    }
    
    bool CheckIfButtonPressed()
    {
        // ✅ Input check using InputActionProperty - works even if XR Origin is disabled
        if (skipActivationAction.action != null)
        {
            return skipActivationAction.action.WasPressedThisFrame();
        }
        
        // Fallback method - only used if skipActivationAction isn't set
        return Input.GetMouseButtonDown(0);
    }
    
    void SkipCutscene()
    {
        // ✅ Keep XR Origin enabled when skipping to main menu
        // Load main menu scene (Scene 1)
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnCutsceneEnd(PlayableDirector director)
    {
        if (director == playableDirector)
        {
            isReturningFromCutscene = true;
            
            // ✅ Enable XR Origin when cutscene ends
            if (xrOrigin != null)
            {
                xrOrigin.SetActive(true);
            }
            
            StartCoroutine(GoToSceneRoutine(sceneIndex));
        }
    }

    public void GoToScene(int sceneIndex)
    {
        if (!useCutscene)
        {
            StartCoroutine(GoToSceneRoutine(sceneIndex));
        }
        else if (playableDirector != null)
        {
            // ✅ Disable XR Origin when starting cutscene
            if (xrOrigin != null)
            {
                xrOrigin.SetActive(false);
            }
            
            playableDirector.Play();
        }
    }

    IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);

        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // ✅ Position player and enable/disable objects after loading
        if (isReturningFromCutscene)
        {
            PositionPlayerAtSpawn();
            ManageGameObjects();
            ManageComponents();
        }

        fadeScreen.FadeIn();
    }

    // ✅ Set player position after returning from cutscene
    private void PositionPlayerAtSpawn()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPosition;
            Debug.Log($"Player repositioned to: {spawnPosition}");
        }
    }

    // ✅ Enable/Disable objects based on state
    private void ManageGameObjects()
    {
        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // ✅ Disable specific objects after returning from cutscene
        if (objectsToDisableAfterCutscene != null)
        {
            foreach (GameObject obj in objectsToDisableAfterCutscene)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    // ✅ Enable specific components
    private void ManageComponents()
    {
        if (componentsToEnable != null)
        {
            foreach (MonoBehaviour component in componentsToEnable)
            {
                if (component != null) component.enabled = true;
            }
        }
    }
}