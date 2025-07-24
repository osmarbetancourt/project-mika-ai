using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.UI;

public class AudioCommandReceiver : MonoBehaviour
{
    public AudioSource audioSource;
    public Button recordButton; // Assign this in the Inspector!

    /// <summary>
    /// Call this method with raw WAV bytes to play the audio.
    /// Returns the duration of the clip (in seconds), or 0 if unknown.
    /// </summary>
    public float PlayWavBytes(byte[] wavData)
    {
        Debug.Log($"[AudioCommandReceiver] PlayWavBytes called with {wavData.Length} bytes at {System.DateTime.Now:HH:mm:ss.fff}");

        // Save bytes to a temporary WAV file
        string tempPath = Path.Combine(Application.persistentDataPath, "temp.wav");
        File.WriteAllBytes(tempPath, wavData);

        // Try to get duration synchronously
        float duration = 0f;
        AudioClip clip = null;
#if UNITY_2018_1_OR_NEWER
        // Use UnityWebRequestMultimedia in newer Unity if you want, but for now use WWW for compatibility
        using (WWW www = new WWW("file://" + tempPath))
        {
            while (!www.isDone) { } // Synchronously wait (fast for local file)
            clip = www.GetAudioClip(false, false, AudioType.WAV);
            if (clip != null)
                duration = clip.length;
        }
#else
        using (WWW www = new WWW("file://" + tempPath))
        {
            while (!www.isDone) { }
            clip = www.GetAudioClip(false, false, AudioType.WAV);
            if (clip != null)
                duration = clip.length;
        }
#endif

        // Start coroutine to load and play the WAV asynchronously (so the UI updates, etc)
        StartCoroutine(LoadAndPlay(tempPath));

        return duration;
    }

    private IEnumerator LoadAndPlay(string filePath)
    {
        Debug.Log($"[AudioCommandReceiver] Loading and playing WAV file at: {filePath} ({System.DateTime.Now:HH:mm:ss.fff})");
        using (WWW www = new WWW("file://" + filePath))
        {
            yield return www;

            AudioClip clip = www.GetAudioClip(false, false, AudioType.WAV);
            if (clip != null)
            {
                Debug.Log("[AudioCommandReceiver] AudioClip loaded successfully, playing...");
                audioSource.clip = clip;
                audioSource.Play();

                // Disable the record button while Mika is talking
                if (recordButton != null)
                    recordButton.interactable = false;

                // Re-enable when Mika finishes speaking
                StartCoroutine(EnableButtonWhenDone(clip.length));
            }
            else
            {
                Debug.LogError("[AudioCommandReceiver] Failed to load audio clip from: " + filePath);
                // If loading fails, re-enable immediately
                if (recordButton != null)
                    recordButton.interactable = true;
            }
        }
    }

    private IEnumerator EnableButtonWhenDone(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (recordButton != null)
            recordButton.interactable = true;
    }
}