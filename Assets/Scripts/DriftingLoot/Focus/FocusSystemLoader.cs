using UnityEngine;

public class FocusSystemLoader : WorldLoader
{
    [SerializeField] private FocusManager focusManager;

    protected override void Load(WorldData worldData)
    {
        var focusData = worldData?.FocusSystem;

        if (focusData != null) {
            focusManager.Init(focusData);
        }
        else {
            focusManager.Init();
        }
    }
}