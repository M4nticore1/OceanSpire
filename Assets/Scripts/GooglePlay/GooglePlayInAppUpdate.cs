using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;
using System.Collections;

public class GooglePlayInAppUpdate : MonoBehaviour
{
    private AppUpdateManager appUpdateManager;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        appUpdateManager = new AppUpdateManager();
        StartCoroutine(CheckForUpdate());
#endif
    }

    private IEnumerator CheckForUpdate()
    {
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful) {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();
            int availability = (int)appUpdateInfoResult.UpdateAvailability;

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable) {
                var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                StartCoroutine(StartImmediateUpdate(appUpdateInfoResult, appUpdateOptions));
            }
            else {
                Debug.Log($"[{nameof(GooglePlayInAppUpdate)}] Обновление недоступно для данного устройства/аккаунта.");
            }
        }
        else {
            Debug.Log($"[{nameof(GooglePlayInAppUpdate)}] Ошибка проверки обновления: {appUpdateInfoOperation.Error}");
        }
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo, AppUpdateOptions appUpdateOptions)
    {
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);

        yield return startUpdateRequest;
    }
}