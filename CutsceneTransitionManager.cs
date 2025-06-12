using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneTransitionManager : MonoBehaviour
{
    public static CutsceneTransitionManager Instance;
    public GameObject faderScreenPrefab; // Assign the fader screen prefab in the Inspector
    private GameObject faderScreenInstance;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        if (faderScreenInstance == null)
        {
            faderScreenInstance = Instantiate(faderScreenPrefab);
        }

        faderScreenInstance.SetActive(true);
        CanvasGroup canvasGroup = faderScreenInstance.GetComponent<CanvasGroup>();

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        if (faderScreenInstance != null)
        {
            CanvasGroup canvasGroup = faderScreenInstance.GetComponent<CanvasGroup>();

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            faderScreenInstance.SetActive(false);
        }
    }
}
