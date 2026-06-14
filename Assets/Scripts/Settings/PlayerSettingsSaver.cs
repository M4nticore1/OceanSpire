using UnityEngine;

public class PlayerSettingsSaver : MonoBehaviour
{
    [SerializeField] private PlayerSettingsManager playerSettingsManager;

    private void OnEnable()
    {
        playerSettingsManager.OnSettingsChanged += OnPlayerSettingsChagned;
    }

    private void OnDisable()
    {
        playerSettingsManager.OnSettingsChanged -= OnPlayerSettingsChagned;
    }

    private void OnPlayerSettingsChagned(PlayerSettingsData playerSettingsData)
    {
        PlayerSettingsSaveSystem.SaveData(playerSettingsData);
    }
}