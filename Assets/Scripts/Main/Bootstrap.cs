using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Start()
    {
        _ = PlayerSettings.Instance;
        _ = SelectManager.Instance;

        SaveManager.Instance.Init();
        AwakeAsync();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    private async void AwakeAsync()
    {
        await LocalizationManager.Instance.InitAsync();
    }
}
