using System;
using UnityEngine;

[Serializable]
public class WandererSystemData
{
    public long? NextWandererTime = null;

    public static WandererSystemData Create(WanderersManager wanderersManager)
    {
        return new WandererSystemData() {
            NextWandererTime = wanderersManager.NextWandererTime,
        };
    }
}