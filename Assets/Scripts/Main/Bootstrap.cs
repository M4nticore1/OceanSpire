using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        _ = PlayerSettingsManager.Instance;
        _ = WorldSaveHandler.Instance;
        _ = BuildingsList.Instance;
        _ = LocalizationManager.Instance;
    }
}