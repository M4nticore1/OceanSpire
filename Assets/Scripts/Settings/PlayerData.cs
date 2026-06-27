using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int Version = 1;
    public PlayerSettingsData Settings = PlayerSettingsData.Default();
    public TutorialData Tutorial;

    public static PlayerData Create(PlayerSettingsManager playerSettings, TutorialManager tutorial)
    {
        return new PlayerData()
        {
            Settings = PlayerSettingsData.Create(playerSettings),
            Tutorial = TutorialData.Create(tutorial)
        };
    }

    public static PlayerData Default()
    {
        return new PlayerData();
    }
}