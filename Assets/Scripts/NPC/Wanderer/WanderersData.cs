using System;
using UnityEngine;

[Serializable]
public class WanderersData
{
    public int Cooldown = 0;
    public int TimeSinceLastSpawn = 0;

    public static WanderersData Create(WanderersManager wanderersManager)
    {
        return new WanderersData() {
            Cooldown = (int)wanderersManager.CurrentWandererSpawnCooldown,
            TimeSinceLastSpawn = (int)wanderersManager.CurrentWandererSpawnTime,
        };
    }
}