using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DebugManager : MonoBehaviour
{
    public Text debugText; // Assign this in the Inspector
    private List<string> messages = new List<string>();

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        messages.Add(logString);
        if (messages.Count > 50)
        {
            messages.RemoveAt(0); // Keep the list size manageable
        }
        debugText.text = string.Join("\n", messages.ToArray());
    }
}