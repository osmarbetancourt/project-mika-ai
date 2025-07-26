using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro; // For TextMeshProUGUI

public class MicRecorder : MonoBehaviour
{
    public Button recordButton;
    public NativeWebSocketExample socketSender; // Assign in Inspector

    private AudioClip recordedClip;
    private bool isRecording = false;
    private string micName;
    private int sampleRate = 16000; // Good for STT

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            SetButtonText("No Mic");
            recordButton.interactable = false;
            return;
        }

        micName = Microphone.devices[0];
        recordButton.onClick.AddListener(ToggleRecording);
        SetButtonText("Record");
    }

    void ToggleRecording()
    {
        if (!isRecording)
            StartRecording();
        else
            StopRecordingAndSend();
    }

    void StartRecording()
    {
        isRecording = true;
        SetButtonText("Stop");
        recordedClip = Microphone.Start(micName, false, 180, sampleRate); // max 180s
    }

    void StopRecordingAndSend()
    {
        if (!isRecording) return;
        isRecording = false;

        int lastSample = Microphone.GetPosition(micName);
        Microphone.End(micName);
        SetButtonText("Sending...");

        float[] samples = new float[recordedClip.samples * recordedClip.channels];
        recordedClip.GetData(samples, 0);

        // Trim to actual recorded length
        if (lastSample > 0 && lastSample < samples.Length)
        {
            float[] trimmed = new float[lastSample * recordedClip.channels];
            Array.Copy(samples, trimmed, trimmed.Length);
            samples = trimmed;
        }

        // Convert to WAV bytes (see WavUtility.cs)
        byte[] wavBytes = WavUtility.FromAudioClip(recordedClip, samples, recordedClip.channels, sampleRate);

        // Send to backend using your websocket
        socketSender.SendAudioBytes(wavBytes);

        // Do NOT re-enable the button here! AudioCommandReceiver will do it.
        SetButtonText("Record");
        recordButton.interactable = false; // Ensure it stays disabled until TTS reply finishes
    }

    void SetButtonText(string text)
    {
        var tmpLabel = recordButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpLabel != null)
        {
            tmpLabel.text = text;
        }
    }
}