using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;
using System.Collections;

public class GooglePlayInAppUpdate : MonoBehaviour
{
    [Header("Update Settings")]
    [Tooltip("True for Flexible (background download), False for Immediate (forced full-screen)")]
    [SerializeField] private bool useFlexibleUpdate = false;
    [SerializeField] private int minUpdatePriority = 0;

    private AppUpdateManager appUpdateManager;
    private AppUpdateInfo appUpdateInfoResult;

    private bool isUpdateChecked = false;
    private bool isUpdateInProgress = false;
    private bool isInitialized = false;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(InitializeWithDelay());
#endif
    }

    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForEndOfFrame();

        appUpdateManager = new AppUpdateManager();
        isInitialized = true;

        StartCoroutine(CheckForUpdate());
    }

    private void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isInitialized || appUpdateManager == null) return;

        if (!pauseStatus) {
            StartCoroutine(DelayedUpdateCheck());
        }
#endif
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        appUpdateManager = null;
#endif
    }

    private IEnumerator DelayedUpdateCheck()
    {
        yield return new WaitForEndOfFrame();

        if (appUpdateManager == null || isUpdateInProgress) yield break;

        if (useFlexibleUpdate) {
            yield return StartCoroutine(ResumeInProgressUpdate());
        }
        else if (!isUpdateChecked) {
            yield return StartCoroutine(CheckForUpdate());
        }
    }

    private IEnumerator CheckForUpdate()
    {
        if (appUpdateManager == null) yield break;

        var appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful) {
            isUpdateChecked = true;
            appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable) {
                int updatePriority = appUpdateInfoResult.UpdatePriority;

                if (updatePriority >= minUpdatePriority) {
                    if (useFlexibleUpdate) {
                        if (appUpdateInfoResult.IsUpdateTypeAllowed(AppUpdateOptions.FlexibleAppUpdateOptions())) {
                            StartCoroutine(StartFlexibleUpdate());
                        }
                        else {
                            Debug.LogWarning("[PlayUpdate] Flexible update not allowed, trying immediate...");
                            if (appUpdateInfoResult.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions())) {
                                StartCoroutine(StartImmediateUpdate());
                            }
                        }
                    }
                    else {
                        if (appUpdateInfoResult.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions())) {
                            StartCoroutine(StartImmediateUpdate());
                        }
                    }
                }
            }
        }
        else {
            Debug.LogError($"[PlayUpdate] Check failed: {appUpdateInfoOperation.Error}");
        }
    }

    private IEnumerator StartFlexibleUpdate()
    {
        isUpdateInProgress = true;
        var appUpdateOptions = AppUpdateOptions.FlexibleAppUpdateOptions();
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoResult, appUpdateOptions);

        yield return startUpdateRequest;

        if (startUpdateRequest.Error == AppUpdateErrorCode.NoError) {
            yield return StartCoroutine(CompleteFlexibleUpdate());
        }
        else {
            Debug.LogError($"[PlayUpdate] Flexible update failed: {startUpdateRequest.Error}");
            isUpdateInProgress = false;
        }
    }

    private IEnumerator CompleteFlexibleUpdate()
    {
        var completeUpdateOperation = appUpdateManager.CompleteUpdate();
        yield return completeUpdateOperation;

        if (completeUpdateOperation.Error != AppUpdateErrorCode.NoError) {
            Debug.LogError($"[PlayUpdate] CompleteUpdate failed: {completeUpdateOperation.Error}");
            isUpdateInProgress = false;
        }
    }

    private IEnumerator StartImmediateUpdate()
    {
        isUpdateInProgress = true;
        var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoResult, appUpdateOptions);

        yield return startUpdateRequest;

        if (startUpdateRequest.Error != AppUpdateErrorCode.NoError) {
            Debug.LogError($"[PlayUpdate] Immediate update failed: {startUpdateRequest.Error}");
            isUpdateInProgress = false;
        }
    }

    private IEnumerator ResumeInProgressUpdate()
    {
        var appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful) {
            var result = appUpdateInfoOperation.GetResult();

            if (result.UpdateAvailability == UpdateAvailability.UpdateAvailable &&
                result.AppUpdateStatus == AppUpdateStatus.Downloaded) {
                yield return StartCoroutine(CompleteFlexibleUpdate());
            }
        }
    }

    public void CheckForUpdateManually()
    {
        if (!isUpdateChecked && !isUpdateInProgress) {
            StartCoroutine(CheckForUpdate());
        }
    }

    public bool IsUpdateAvailable()
    {
        return appUpdateInfoResult != null && appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable;
    }

    public int GetUpdatePriority()
    {
        return appUpdateInfoResult != null ? appUpdateInfoResult.UpdatePriority : 0;
    }

    public int GetClientStalenessDays()
    {
        if (appUpdateInfoResult == null) return -1;

        return appUpdateInfoResult.ClientVersionStalenessDays ?? 0;
    }
}