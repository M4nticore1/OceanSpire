using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int Version = 1;
    public PlayerSettingsData Settings = PlayerSettingsData.Default();
    public TutorialData Tutorial;

    public void UpdateSettings(PlayerSettingsManager playerSettings)
    {
        Settings = PlayerSettingsData.Create(playerSettings);
    }

    public void UpdateTutorial(TutorialManager tutorial)
    {
        Tutorial = TutorialData.Create(tutorial);
    }

    public static PlayerData Default()
    {
        return new PlayerData();
    }
}