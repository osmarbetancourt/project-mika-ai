using UnityEngine;
using System;
using System.IO;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip, float[] samples, int channels, int sampleRate)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        // Convert floats to 16-bit PCM
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        using (var memStream = new MemoryStream())
        {
            // WAV header
            int fileSize = 44 + bytesData.Length;
            memStream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            memStream.Write(BitConverter.GetBytes(fileSize - 8), 0, 4);
            memStream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            memStream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
            memStream.Write(BitConverter.GetBytes(16), 0, 4);
            memStream.Write(BitConverter.GetBytes((short)1), 0, 2);
            memStream.Write(BitConverter.GetBytes((short)channels), 0, 2);
            memStream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
            memStream.Write(BitConverter.GetBytes(sampleRate * channels * 2), 0, 4);
            memStream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
            memStream.Write(BitConverter.GetBytes((short)16), 0, 2);
            memStream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
            memStream.Write(BitConverter.GetBytes(bytesData.Length), 0, 4);
            memStream.Write(bytesData, 0, bytesData.Length);
            return memStream.ToArray();
        }
    }
}