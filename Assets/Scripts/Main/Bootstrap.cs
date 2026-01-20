using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        SaveManager.Instance.Initialize();
        AwakeAsync();
    }

    private async void AwakeAsync()
    {
        await LocalizationManager.Instance.InitializeAsync();
    }
}
