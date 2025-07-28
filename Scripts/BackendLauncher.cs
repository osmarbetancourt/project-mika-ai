using System.Diagnostics;
using UnityEngine;

public class BackendLauncher : MonoBehaviour
{
    public string backendExeName = "server.exe"; // Name of your backend exe
    public string backendExeRelativePath = "server.exe"; // Path relative to the Unity build

    private Process backendProcess = null;

    void Start()
    {
        StartBackendIfNeeded();
    }

    void StartBackendIfNeeded()
    {
        var running = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(backendExeName));
        if (running.Length > 0)
        {
            UnityEngine.Debug.Log("[Mika] Backend already running.");
            return;
        }

        var psi = new ProcessStartInfo();
        psi.FileName = System.IO.Path.Combine(Application.dataPath, "..", backendExeRelativePath);
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;
        psi.WindowStyle = ProcessWindowStyle.Hidden;

        try
        {
            backendProcess = Process.Start(psi);
            UnityEngine.Debug.Log("[Mika] Backend started.");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("[Mika] Could not start backend: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (backendProcess != null && !backendProcess.HasExited)
        {
            try
            {
                backendProcess.Kill();
                backendProcess.Dispose();
                UnityEngine.Debug.Log("[Mika] Backend process killed on exit.");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("[Mika] Could not kill backend: " + e.Message);
            }
        }
    }
}