using UnityEngine;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTransitionController : MonoBehaviour
{
    #if UNITY_EDITOR
    public SceneAsset mainSceneAsset; // Assign the SceneAsset in the Inspector
    #endif

    private string mainSceneName;
    private CutsceneTransitionManager cutsceneTransitionManager;

    void Start()
    {
        #if UNITY_EDITOR
        if (mainSceneAsset != null)
        {
            mainSceneName = mainSceneAsset.name;
            Debug.Log($"Main Scene Asset Name: {mainSceneName}");
        }
        else
        {
            Debug.LogError("Main Scene Asset not assigned.");
            return;
        }
        #endif

        // Find the existing CutsceneTransitionManager in the scene
        cutsceneTransitionManager = FindObjectOfType<CutsceneTransitionManager>();

        if (cutsceneTransitionManager == null)
        {
            Debug.LogError("CutsceneTransitionManager not found in the scene.");
            return;
        }

        // Start playing the Timeline
        PlayableDirector playableDirector = GetComponent<PlayableDirector>();
        if (playableDirector != null)
        {
            playableDirector.played += OnPlayableDirectorPlayed;
            playableDirector.stopped += OnPlayableDirectorStopped;
            playableDirector.Play();
            Debug.Log("PlayableDirector started playing.");
        }
        else
        {
            Debug.LogError("PlayableDirector component not found.");
        }
    }

    void OnPlayableDirectorPlayed(PlayableDirector director)
    {
        Debug.Log("Cutscene started.");
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        Debug.Log("Cutscene finished.");
        // Load the main game scene using CutsceneTransitionManager
        if (!string.IsNullOrEmpty(mainSceneName))
        {
            cutsceneTransitionManager.GoToSceneAsync(mainSceneName);
            Debug.Log($"Loading main scene: {mainSceneName}");
        }
        else
        {
            Debug.LogError("Main scene name not set.");
        }
    }
}
