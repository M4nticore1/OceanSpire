using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        _ = PlayerSettings.Instance;
        _ = SelectManager.Instance;
        new LocalizationManager();

        SaveManager.Instance.Initialize();
        AwakeAsync();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    private async void AwakeAsync()
    {
        await LocalizationManager.Instance.InitializeAsync();
    }
}
