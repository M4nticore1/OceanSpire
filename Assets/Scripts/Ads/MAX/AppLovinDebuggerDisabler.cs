using UnityEngine;

public class AppLovinDebuggerDisabler : MonoBehaviour
{
    private void Awake()
    {
        MaxSdk.SetCreativeDebuggerEnabled(false);
    }
}