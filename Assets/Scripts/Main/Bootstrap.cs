using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        _ = PlayerSettings.Instance;

        _ = WorldSaveManager.Instance;
        _ = SettingsSaveManager.Instance;

        _ = BuildingsList.Instance;

        _ = LocalizationManager.Instance;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}