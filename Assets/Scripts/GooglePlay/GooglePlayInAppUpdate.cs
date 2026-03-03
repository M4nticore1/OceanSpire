using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;
using TMPro;
using System.Collections;

public class GooglePlayInAppUpdate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inAppStatus;
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

        var appUpdateInforResult = appUpdateInfoOperation.GetResult();

        if (appUpdateInfoOperation.IsSuccessful) {
            if (appUpdateInforResult.UpdateAvailability == UpdateAvailability.UpdateAvailable) {
                inAppStatus.SetText(UpdateAvailability.UpdateAvailable.ToString());
            }
            else {
                inAppStatus.SetText("No Update Avaliable");
            }
        }

        var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();

        StartCoroutine(StartImmediateUpdate(appUpdateInforResult, appUpdateOptions));
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo, AppUpdateOptions appUpdateOptions)
    {
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);

        yield return startUpdateRequest;
    }
}