using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        _ = PlayerSettingsManager.Instance;
        _ = WorldSaveManager.Instance;
        _ = BuildingsList.Instance;
        _ = LocalizationManager.Instance;
    }
}