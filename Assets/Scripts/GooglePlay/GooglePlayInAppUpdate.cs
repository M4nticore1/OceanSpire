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

    private void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    if (!isInitialized || appUpdateManager == null) return;

    if (hasFocus) {
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
                        else if (appUpdateInfoResult.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions())) {
                            StartCoroutine(StartImmediateUpdate());
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
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Check updates failed: {appUpdateInfoOperation.Error}");
        }
    }

    private IEnumerator StartFlexibleUpdate()
    {
        isUpdateInProgress = true;

        if (appUpdateInfoResult.AppUpdateStatus == AppUpdateStatus.Downloaded) {
            yield return StartCoroutine(CompleteFlexibleUpdate());
            yield break;
        }

        var appUpdateOptions = AppUpdateOptions.FlexibleAppUpdateOptions();
        AppUpdateRequest startUpdateRequest = null;

        try {
            startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoResult, appUpdateOptions);
        }
        catch (System.Exception ex) {
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Native crash on starting Flexible update: {ex.Message}");
            isUpdateInProgress = false;
            yield break;
        }

        if (startUpdateRequest == null) {
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Failed to create StartUpdate operation");
            isUpdateInProgress = false;
            yield break;
        }

        while (!startUpdateRequest.IsDone) {
            if (startUpdateRequest.Error != AppUpdateErrorCode.NoError) {
                Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Error during download process: {startUpdateRequest.Error}");
                isUpdateInProgress = false;
                yield break;
            }

            yield return null;
        }

        if (startUpdateRequest.Status == AppUpdateStatus.Downloaded) {
            yield return StartCoroutine(CompleteFlexibleUpdate());
        }
        else {
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Download finished with unexpected status: {startUpdateRequest.Status}");
            isUpdateInProgress = false;
        }
    }

    private IEnumerator CompleteFlexibleUpdate()
    {
        if (appUpdateInfoResult.AppUpdateStatus != AppUpdateStatus.Downloaded) {
            isUpdateInProgress = false;
            yield break;
        }

        var completeUpdateOperation = appUpdateManager.CompleteUpdate();
        yield return completeUpdateOperation;

        if (completeUpdateOperation.Error != AppUpdateErrorCode.NoError) {
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Complete update failed: {completeUpdateOperation.Error}");
        }

        isUpdateInProgress = false;
    }

    private IEnumerator StartImmediateUpdate()
    {
        isUpdateInProgress = true;
        var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
        AppUpdateRequest startUpdateRequest = null;

        try {
            startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoResult, appUpdateOptions);
        }
        catch (AndroidJavaException ex) {
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Native crash Google Play Core at the start: {ex.Message}");
            isUpdateInProgress = false;
            yield break;
        }

        yield return startUpdateRequest;

        if (startUpdateRequest.Error != AppUpdateErrorCode.NoError) {
            Debug.LogError($"[{nameof(GooglePlayInAppUpdate)}] Immediate update failed: {startUpdateRequest.Error}");
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