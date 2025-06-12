using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ButtonSceneTransition : MonoBehaviour
{
    [Header("Fade Screen Reference")]
    // Drag the GameObject that has your working FadeScreen component (with the Renderer) here.
    public FadeScreen fadeScreen;
    
    #if UNITY_EDITOR
    [Header("Scene to Load (Drag Scene Asset)")]
    public SceneAsset sceneToLoad;
    #endif

    public void OnButtonPressed()
    {
        if (fadeScreen == null)
        {
            Debug.LogError("FadeScreen reference not assigned. Please assign the proper FadeScreen instance.");
            return;
        }

        #if UNITY_EDITOR
        if (sceneToLoad == null)
        {
            Debug.LogError("No scene asset assigned. Please drag the scene asset in the Inspector.");
            return;
        }
        
        string scenePath = AssetDatabase.GetAssetPath(sceneToLoad);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        StartCoroutine(Transition(sceneName));
        #else
        Debug.LogError("Scene asset field is only available in the Editor. Please assign a scene name for builds.");
        #endif
    }

    IEnumerator Transition(string sceneName)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);
        SceneManager.LoadScene(sceneName);
    }
}
