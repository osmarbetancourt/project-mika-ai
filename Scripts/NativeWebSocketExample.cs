using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections.Generic;

public class NativeWebSocketExample : MonoBehaviour
{
    WebSocket websocket;
    public MikaExpressionController mikaExpressionController;
    public AnimationCommandReceiver animationReceiver;
    public AudioCommandReceiver audioReceiver;

    private Queue<byte[]> audioQueue = new Queue<byte[]>();
    private bool audioIsPlaying = false;

    async void Start()
    {
        websocket = new WebSocket("ws://localhost:3000");

        websocket.OnOpen += () => Debug.Log("[WebSocket] Connection open!");
        websocket.OnError += (e) => Debug.Log("[WebSocket] Error! " + e);
        websocket.OnClose += (e) => Debug.Log("[WebSocket] Connection closed!");

        websocket.OnMessage += (bytes) =>
        {
            string message = "";
            bool isText = false;

            if (bytes.Length > 0 && IsProbablyText(bytes))
            {
                message = Encoding.UTF8.GetString(bytes);
                isText = true;
            }

            if (isText && (message.StartsWith("expression:") || message.StartsWith("animation:")))
            {
                Debug.Log("[WebSocket] Received OnMessage! " + message);
                HandleCommand(message);
            }
            else
            {
                Debug.Log($"[WebSocket] Received audio data ({bytes.Length} bytes) at {System.DateTime.Now:HH:mm:ss.fff}");
                lock (audioQueue) audioQueue.Enqueue(bytes);
                TryPlayNextAudio();
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    void HandleCommand(string command)
    {
        Debug.Log("[WebSocket] HandleCommand called with: " + command);

        if (command.StartsWith("expression:"))
        {
            string exprPart = command.Substring("expression:".Length).Trim();
            string expr = exprPart;
            float duration = 2f;

            int colonIdx = exprPart.IndexOf(':');
            if (colonIdx > 0)
            {
                expr = exprPart.Substring(0, colonIdx).Trim();
                string durStr = exprPart.Substring(colonIdx + 1).Trim();
                if (!float.TryParse(durStr, out duration))
                {
                    duration = 2f;
                }
            }

            Debug.Log($"[WebSocket] Parsed expression: {expr} (duration {duration}s)");
            mikaExpressionController?.SetExpression(expr, duration);
        }
        else if (command.StartsWith("animation:"))
        {
            string anim = command.Substring("animation:".Length).Trim();
            Debug.Log("[WebSocket] Parsed animation: " + anim);
            animationReceiver?.PlayAnimation(anim);
        }
        else
        {
            Debug.LogWarning("[WebSocket] Unknown command: " + command);
        }
    }

    public async void SendAudioBytes(byte[] wavBytes)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Send(wavBytes);
            Debug.Log($"[WebSocket] Sent audio bytes ({wavBytes.Length} bytes)");
        }
        else
        {
            Debug.LogWarning("[WebSocket] WebSocket not connected, can't send audio.");
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    bool IsProbablyText(byte[] bytes)
    {
        int checkLen = Mathf.Min(8, bytes.Length);
        for (int i = 0; i < checkLen; i++)
        {
            byte b = bytes[i];
            if ((b < 32 || b > 126) && b != 9 && b != 10 && b != 13)
                return false;
        }
        return true;
    }

    // ---- AUDIO PLAYBACK QUEUE LOGIC ----

    void TryPlayNextAudio()
    {
        if (audioIsPlaying) return;
        byte[] nextAudio = null;
        lock (audioQueue)
        {
            if (audioQueue.Count > 0) nextAudio = audioQueue.Dequeue();
        }
        if (nextAudio != null)
        {
            StartCoroutine(PlayAudioCoroutine(nextAudio));
        }
    }

    System.Collections.IEnumerator PlayAudioCoroutine(byte[] audioBytes)
    {
        audioIsPlaying = true;
        if (audioReceiver != null)
        {
            float duration = audioReceiver.PlayWavBytes(audioBytes); // <-- see below
            if (duration <= 0) duration = 5f; // fallback if you can't get duration
            yield return new WaitForSeconds(duration + 0.1f);
        }
        else
        {
            Debug.LogWarning("[WebSocket] No AudioCommandReceiver assigned!");
            yield return new WaitForSeconds(1f);
        }
        audioIsPlaying = false;
        TryPlayNextAudio();
    }
}