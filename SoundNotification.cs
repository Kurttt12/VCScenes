using System.Collections.Generic;
using UnityEngine;

public class SoundNotification : MonoBehaviour
{
    // A small helper class to store your audio data in a named entry.
    [System.Serializable]
    public class AudioEntry
    {
        public string audioName;     // The name used for identifying the audio.
        public AudioSource audioSource; // A reference to the AudioSource to play.
    }

    // Make a list so you can add as many sounds as needed.
    public List<AudioEntry> audioEntries = new List<AudioEntry>();

    // Singleton instance to allow other scripts to access PlaySound easily.
    public static SoundNotification Instance { get; private set; }

    private void Awake()
    {
        // If no Instance exists, make this our singleton instance.
        // Otherwise, destroy the duplicate.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays a sound based on the provided name.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    public void PlaySound(string soundName)
    {
        // Search for the audio entry with the specified name.
        AudioEntry entry = audioEntries.Find(item => item.audioName == soundName);
        if (entry != null)
        {
            entry.audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Sound '" + soundName + "' not found in SoundNotification list.");
        }
    }
}
