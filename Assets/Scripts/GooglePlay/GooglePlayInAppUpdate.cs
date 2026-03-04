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
            var appUpdateInforResult = appUpdateInfoOperation.GetResult();
            var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();

            StartCoroutine(StartImmediateUpdate(appUpdateInforResult, appUpdateOptions));
        }
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo, AppUpdateOptions appUpdateOptions)
    {
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);

        yield return startUpdateRequest;
    }
}