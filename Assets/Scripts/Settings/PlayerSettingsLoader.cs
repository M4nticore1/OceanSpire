using UnityEngine;

public class PlayerSettingsLoader : MonoBehaviour
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

    private void Start()
    {
        var playerSettingsData = PlayerSettingsSaveSystem.GetData();
        if (playerSettingsData == null) {
            playerSettingsData = PlayerSettingsData.Default();
        }

        playerSettings.Init(playerSettingsData);
    }
}