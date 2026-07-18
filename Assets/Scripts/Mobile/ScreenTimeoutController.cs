using UnityEngine;

public class ScreenTimeoutController : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SetNeverSleep(true);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetNeverSleep(hasFocus);
    }

    private void SetNeverSleep(bool neverSleep)
    {
        Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
    }
}