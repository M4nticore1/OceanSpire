using UnityEngine;

public class SettingsSaveManager
{
    private static SettingsSaveManager instance;
    public static SettingsSaveManager Instance
    {
        get
        {
            if (instance == null) {
                instance = new SettingsSaveManager();
                instance.Init();
            }

            return instance;
        }
    }

    public SettingsData savedData { get; private set; } = null;
    private bool isInited = false;

    private void Init()
    {
        if (isInited) return;

        AssignData();
        isInited = true;
    }

    private void AssignData()
    {
        savedData = SettingsSaveSystem.GetData();
    }
}
