using UnityEngine;
using UniVRM10;

public class SimpleLipSync : MonoBehaviour
{
    public AudioSource audioSource; // Assign in Inspector
    public Vrm10Instance vrmInstance; // Assign in Inspector or GetComponent<>
    public string mouthShape = "aa"; // VRM preset for open mouth
    public float sensitivity = 80.0f;
    public float smoothing = 0.1f;
    private float mouthOpen = 0f;

    void Update()
    {
        if (audioSource != null && vrmInstance != null && audioSource.isPlaying)
        {
            // Get current audio amplitude
            float[] samples = new float[256];
            audioSource.GetOutputData(samples, 0);
            float sum = 0f;
            for (int i = 0; i < samples.Length; ++i)
                sum += samples[i] * samples[i];
            float rms = Mathf.Sqrt(sum / samples.Length);
            float targetOpen = Mathf.Clamp01(rms * sensitivity);

            // Smooth
            mouthOpen = Mathf.Lerp(mouthOpen, targetOpen, smoothing);

            // Set VRM expression weight (mouth open)
            vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), mouthOpen);
        }
        else if (vrmInstance != null)
        {
            // Close mouth when no audio
            vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), 0f);
        }
    }
}