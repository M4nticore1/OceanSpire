using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        _ = PlayerSettings.Instance;
        _ = SelectManager.Instance;

        WorldSaveManager.Instance.Init();
        SettingsSaveManager.Instance.Init();
        SettingsData data = SettingsSaveManager.Instance.savedData;

        BuildingsList.Instance.Init();

        LocalizationManager.Instance.Init(null);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}
