using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Robust version: tracks all pending audio (fallback + real answer),
/// only re-enables the record button after ALL audio responses have finished playing.
/// </summary>
public class AudioCommandReceiver : MonoBehaviour
{
    public AudioSource audioSource;
    public Button recordButton; // Assign this in the Inspector!

    // This queue tracks all the audio files that need to be played (for multi-audio responses)
    private Queue<string> pendingAudioFiles = new Queue<string>();
    private bool isPlaying = false;

    /// <summary>
    /// Call this method with raw WAV bytes to play the audio.
    /// Returns the duration of the clip (in seconds), or 0 if unknown.
    /// </summary>
    public float PlayWavBytes(byte[] wavData)
    {
        Debug.Log($"[AudioCommandReceiver] PlayWavBytes called with {wavData.Length} bytes at {System.DateTime.Now:HH:mm:ss.fff}");

        // Save bytes to a temporary WAV file with a unique name
        string tempPath = Path.Combine(Application.persistentDataPath, "mika_temp_" + System.Guid.NewGuid().ToString() + ".wav");
        File.WriteAllBytes(tempPath, wavData);

        // Enqueue the audio file path for sequential playback
        pendingAudioFiles.Enqueue(tempPath);

        // If nothing is playing, start coroutine to play the queue
        if (!isPlaying)
            StartCoroutine(PlayPendingAudioQueue());

        // Duration estimate is best-effort (not always reliable)
        float duration = 0f;
#if UNITY_2018_1_OR_NEWER
        using (WWW www = new WWW("file://" + tempPath))
        {
            while (!www.isDone) { }
            AudioClip clip = www.GetAudioClip(false, false, AudioType.WAV);
            if (clip != null)
                duration = clip.length;
        }
#else
        using (WWW www = new WWW("file://" + tempPath))
        {
            while (!www.isDone) { }
            AudioClip clip = www.GetAudioClip(false, false, AudioType.WAV);
            if (clip != null)
                duration = clip.length;
        }
#endif
        return duration;
    }

    /// <summary>
    /// Plays all pending audio responses in sequence, disabling the record button until done.
    /// </summary>
    private IEnumerator PlayPendingAudioQueue()
    {
        isPlaying = true;

        // Disable the record button while Mika is talking
        if (recordButton != null)
            recordButton.interactable = false;

        while (pendingAudioFiles.Count > 0)
        {
            string filePath = pendingAudioFiles.Dequeue();
            yield return StartCoroutine(LoadAndPlay(filePath));
            // Optionally delete temp file after use
            try { File.Delete(filePath); } catch { }
        }

        // After all audio is done, enable the button again ONLY if mic and connection are still OK
        var micRecorder = FindObjectOfType<MicRecorder>();
        bool micPresent = micRecorder != null && Microphone.devices.Length > 0;
        bool connected = micRecorder != null && micRecorder.socketSender != null && micRecorder.socketSender.IsConnected;
        if (recordButton != null)
        {
            recordButton.interactable = micPresent && connected;
            if (micRecorder != null && micPresent)
                micRecorder.SetButtonText("Record");
        }

        isPlaying = false;
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

                // Wait until audio is done
                yield return new WaitForSeconds(clip.length);
            }
            else
            {
                Debug.LogError("[AudioCommandReceiver] Failed to load audio clip from: " + filePath);
                // If loading fails, just continue
            }
        }
    }
}