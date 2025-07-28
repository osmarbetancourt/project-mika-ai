using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections.Generic;
using System; // for Exception
using System.Collections;

public class NativeWebSocketExample : MonoBehaviour
{
    WebSocket websocket;
    public MikaExpressionController mikaExpressionController;
    public AnimationCommandReceiver animationReceiver;
    public AudioCommandReceiver audioReceiver;

    private Queue<(byte[], string)> audioQueue = new Queue<(byte[], string)>();
    private bool audioIsPlaying = false;

    // Pending animation/expression for next audio
    private string pendingAnimation = null;
    private string pendingExpression = null;
    private float pendingExpressionDuration = 2f;

    // Track flag from server
    private string nextAudioType = null; // "fallback" or "response"

    private bool websocketConnected = false;
    private bool tryingToConnect = false;

    // Expose connection state for UI/scripts
    public bool IsConnected => websocket != null && websocket.State == WebSocketState.Open;

    private void Start()
    {
        StartCoroutine(ConnectWithRetry());
    }

    private IEnumerator ConnectWithRetry()
    {
        // Retry connection until websocket is connected
        while (!websocketConnected)
        {
            if (!tryingToConnect)
            {
                tryingToConnect = true;
                Debug.Log("[WebSocket] Attempting to connect...");
                websocket = new WebSocket("ws://localhost:3000");

                websocket.OnOpen += () =>
                {
                    websocketConnected = true;
                    Debug.Log("[WebSocket] Connection open!");
                };
                websocket.OnError += (e) => Debug.Log("[WebSocket] Error! " + e);
                websocket.OnClose += (e) =>
                {
                    websocketConnected = false;
                    Debug.Log("[WebSocket] Connection closed!");
                };

                websocket.OnMessage += (bytes) =>
                {
                    bool isText = false;
                    string message = "";

                    if (bytes.Length > 0 && IsProbablyText(bytes))
                    {
                        message = Encoding.UTF8.GetString(bytes);
                        isText = true;
                    }

                    if (isText)
                    {
                        if (message.StartsWith("audio:"))
                        {
                            nextAudioType = message.Substring("audio:".Length).Trim(); // fallback or response
                            Debug.Log("[WebSocket] Next audio type set to: " + nextAudioType);
                        }
                        else if (message.StartsWith("expression:") || message.StartsWith("animation:"))
                        {
                            Debug.Log("[WebSocket] Received OnMessage! " + message);
                            HandleCommand(message);
                        }
                    }
                    else
                    {
                        if (nextAudioType == null)
                        {
                            nextAudioType = "response"; // default if not set
                            Debug.Log("[WebSocket] No audio type set, using default 'response'.");
                        }
                        audioQueue.Enqueue((bytes, nextAudioType));
                        Debug.Log($"[WebSocket] Received audio data ({bytes.Length} bytes), type: {nextAudioType} at {System.DateTime.Now:HH:mm:ss.fff}");
                        nextAudioType = null;
                        TryPlayNextAudio();
                    }
                };

                // Try to connect asynchronously
                var task = websocket.Connect();
                while (!task.IsCompleted)
                    yield return null;

                if (!websocketConnected)
                {
                    Debug.Log("[WebSocket] Connection failed, retrying in 1s...");
                    yield return new WaitForSeconds(1f);
                    tryingToConnect = false;
                }
            }
            else
            {
                // Wait a bit before next attempt if another try is ongoing
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    // Store commands for next audio playback
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
            pendingExpression = expr;
            pendingExpressionDuration = duration;
        }
        else if (command.StartsWith("animation:"))
        {
            string anim = command.Substring("animation:".Length).Trim();
            Debug.Log("[WebSocket] Parsed animation: " + anim);
            pendingAnimation = anim;
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
        if (websocket != null)
        {
            await websocket.Close();
        }
        else
        {
            Debug.LogWarning("[WebSocket] websocket was null during OnApplicationQuit.");
        }
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
        (byte[] audioBytes, string audioType) nextAudio = (null, null);
        if (audioQueue.Count > 0) nextAudio = audioQueue.Dequeue();
        if (nextAudio.audioBytes != null)
        {
            StartCoroutine(PlayAudioCoroutine(nextAudio.audioBytes, nextAudio.audioType));
        }
    }

    // After starting audio playback, trigger pending animation/expression
    IEnumerator PlayAudioCoroutine(byte[] audioBytes, string audioType)
    {
        audioIsPlaying = true;
        if (audioReceiver != null)
        {
            float duration = audioReceiver.PlayWavBytes(audioBytes);
            if (duration <= 0) duration = 5f;

            if (audioType == "fallback")
            {
                // Always trigger fallback visuals for fallback audio
                Debug.Log("[WebSocket] Fallback audio detected (flag), forcing Thinking/blink animation/expression.");
                animationReceiver?.PlayAnimation("Thinking");
                mikaExpressionController?.SetExpression("blink", 2.5f);
                pendingAnimation = null;
                pendingExpression = null;
                pendingExpressionDuration = 2f;
            }
            else // response
            {
                if (!string.IsNullOrEmpty(pendingAnimation))
                {
                    animationReceiver?.PlayAnimation(pendingAnimation);
                    pendingAnimation = null;
                }
                if (!string.IsNullOrEmpty(pendingExpression))
                {
                    mikaExpressionController?.SetExpression(pendingExpression, pendingExpressionDuration);
                    pendingExpression = null;
                    pendingExpressionDuration = 2f;
                }
            }

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