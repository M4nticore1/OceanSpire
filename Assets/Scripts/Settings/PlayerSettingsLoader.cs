using UnityEngine;

public class PlayerSettingsLoader : PlayerLoader
{
    public static PlayerSettingsLoader Instance;

    [SerializeField] private PlayerSettingsManager playerSettings;

    private void Awake()
    {
        if (Instance) {
            Debug.Log("Another Player Settings Loader is already on the scene");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Load(PlayerData playerData)
    {
        var playerSettingsData = playerData?.Settings;

        if (playerSettingsData != null) {
            playerSettings.Init(playerSettingsData);
        }
        else {
            playerSettings.Init();
        }
    }
}