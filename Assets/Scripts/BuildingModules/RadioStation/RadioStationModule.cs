using UnityEngine;

public class RadioStationModule : BuildingModule
{
    public RadioStationLevelData RadioStationLevelData => LevelData ? LevelData as RadioStationLevelData : null;

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