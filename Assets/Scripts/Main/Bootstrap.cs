using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        SelectManager selectManager = SelectManager.Instance;
        new SaveManager();
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
