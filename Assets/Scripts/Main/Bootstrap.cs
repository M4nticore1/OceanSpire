using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        _ = WorldSaveHandler.Instance;
        _ = LocalizationManager.Instance;
    }
}