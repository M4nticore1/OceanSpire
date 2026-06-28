using UnityEngine;
using Firebase.Crashlytics;

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
        if (type == LogType.Error || type == LogType.Exception) {
            System.Exception exception = new System.Exception(logString);

            //Crashlytics.Log($"Unity Log: {logString}");
            //Crashlytics.LogException(exception);
        }
    }
}