using UnityEngine;

public class RadioStationModule : BuildingModule, IWanderersCooldownProvider
{
    public RadioStationLevelData RadioStationLevelData => LevelData as RadioStationLevelData;

    // IWanderersCooldownProvider
    public float WanderersCooldownReduction => RadioStationLevelData != null ? RadioStationLevelData.WandererCooldownSpeedBonus : 0f;

    private RadioStationsManager radioStationsManager => RadioStationsManager.Instance;

    protected override void OnEnable()
    {
        base.OnEnable();

        radioStationsManager.RegisterRadioStation(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        radioStationsManager.UnregisterRadioStation(this);
    }
}