using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L_Notification : MonoBehaviour
{
    // A small helper class to store your audio data in a named entry.
    [System.Serializable]
    public class AudioEntry
    {
        public string audioName;      // The name used for identifying the audio.
        public AudioSource audioSource; // A reference to the AudioSource to play.
        public GameObject audioImageObject; // Optional object for the audio entry.
    }

    // Make a list so you can add as many sounds as needed.
    public List<AudioEntry> audioEntries = new List<AudioEntry>();

    // Singleton instance to allow other scripts to access PlaySound easily.
    public static L_Notification Instance { get; private set; }

    private void Awake()
    {
        // If no Instance exists, make this our singleton instance.
        // Otherwise, destroy the duplicate.
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Plays a sound based on the provided name and shows the associated image while the audio plays,
    /// except for sounds that should be ignored (e.g., "incorrect").
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    public void PlaySound(string soundName)
    {
        // Search for the audio entry with the specified name.
        AudioEntry entry = audioEntries.Find(item => item.audioName == soundName);
        if (entry != null)
        {
            // Play the audio.
            entry.audioSource.Play();

            // If this sound should show an image, start the coroutine.
            if (entry.audioImageObject != null && soundName != "incorrect")
            {
                StartCoroutine(ShowDuringPlayback(entry));
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in L_Notification list.");
        }
    }

    /// <summary>
    /// Coroutine to display the image object while the audio is playing, then hide it.
    /// </summary>
    private IEnumerator ShowDuringPlayback(AudioEntry entry)
    {
        entry.audioImageObject.SetActive(true);
        // Wait while the audio is playing.
        yield return new WaitWhile(() => entry.audioSource.isPlaying);
        // Disable the object once playback finishes.
        entry.audioImageObject.SetActive(false);
    }
}
