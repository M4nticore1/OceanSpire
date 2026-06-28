using System;
using UnityEngine;

#if !UNITY_EDITOR
using Firebase.Crashlytics;
#endif

public class CrashlyticsLogger : MonoBehaviour
{
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
        if (type == LogType.Error) {
            Crashlytics.Log($"[{type}] {logString}");
            Crashlytics.LogException(new Exception($"{logString}\n{stackTrace}"));
        }
#endif
    }
}