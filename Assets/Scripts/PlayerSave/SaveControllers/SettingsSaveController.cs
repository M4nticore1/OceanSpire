using UnityEngine;

public class SettingsSaveController : PlayerSaveController
{
    [Header("Settings")]
    [SerializeField] private PlayerSettingsManager playerSettingsManager;
    [SerializeField] private PlayerSettingsLoader playerSettingsLoader;

    protected override void OnSubscribe()
    {
        base.OnSubscribe();

        playerSettingsManager.OnSettingsChanged += OnPlayerSettingsChanged;
    }

    protected override void OnUnsubscribe()
    {
        base.OnUnsubscribe();

        playerSettingsManager.OnSettingsChanged -= OnPlayerSettingsChanged;
    }

    private void OnPlayerSettingsChanged()
    {
        if (!playerSettingsLoader.IsLoaded) return;

        var currentData = PlayerSaveSystem.GetData();
        if (currentData == null) {
            currentData = PlayerData.Default();
        }

        currentData.Settings = PlayerSettingsData.Create(playerSettingsManager);
        PlayerSaveSystem.SaveData(currentData);
    }
}