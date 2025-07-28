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

    void Update()
    {
        bool micPresent = Microphone.devices.Length > 0;
        bool connected = socketSender != null && socketSender.IsConnected;

        // Only allow button to be enabled when:
        // - There is a mic
        // - We are recording (so user can click "Stop")
        // - Or after Mika is done and connected (AudioCommandReceiver will re-enable)
        if (!micPresent)
        {
            recordButton.interactable = false;
            SetButtonText("No Mic");
        }
        else if (isRecording)
        {
            recordButton.interactable = true;
            SetButtonText("Stop");
        }
        else if (!connected)
        {
            // Not connected yet, keep disabled!
            recordButton.interactable = false;
            SetButtonText("Record");
        }
        // else: let AudioCommandReceiver control interactable after Mika is done talking
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
        recordButton.interactable = false;

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
        // SetButtonText("Record"); // Don't set here; AudioCommandReceiver sets it after Mika finishes
        // recordButton.interactable = false; // Not needed anymore; handled in Update() and AudioCommandReceiver
    }

    public void SetButtonText(string text)
    {
        var tmpLabel = recordButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpLabel != null)
        {
            tmpLabel.text = text;
        }
    }
}