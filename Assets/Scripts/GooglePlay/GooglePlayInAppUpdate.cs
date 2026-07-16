using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;
using System.Collections;

public class GooglePlayInAppUpdate : MonoBehaviour
{
    private AppUpdateManager appUpdateManager;
    private bool isUpdateChecked = false;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        appUpdateManager = new AppUpdateManager();
        StartCoroutine(CheckForUpdateWithDelay());
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (pauseStatus)
        {
            // Обнуляем ссылку при сворачивании, чтобы предотвратить вызов NullPointerException во внутреннем коде плагина
            appUpdateManager = null;
        }
        else
        {
            if (appUpdateManager == null)
            {
                appUpdateManager = new AppUpdateManager();
                
                if (!isUpdateChecked)
                {
                    StartCoroutine(CheckForUpdateWithDelay());
                }
            }
        }
#endif
    }

    IEnumerator CheckForUpdateWithDelay()
    {
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(CheckForUpdate());
    }

    private IEnumerator CheckForUpdate()
    {
        if (appUpdateManager == null) yield break;

        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful) {
            isUpdateChecked = true;
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable) {
                var appUpdateOptions = AppUpdateOptions.FlexibleAppUpdateOptions();
                StartCoroutine(StartImmediateUpdate(appUpdateInfoResult, appUpdateOptions));
                Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] The update is available");
            }
            else {
                Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] The update is not available for this device/account..");
            }
        }
        else {
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Update verification error: {appUpdateInfoOperation.Error}");
        }
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo, AppUpdateOptions appUpdateOptions)
    {
        if (appUpdateManager == null) yield break;

        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);
        yield return startUpdateRequest;
    }
}