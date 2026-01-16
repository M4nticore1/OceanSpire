using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        AwakeAsync();
    }

    private async void AwakeAsync()
    {
        await LocalizationManager.Instance.InitializeAsync();
    }
}
