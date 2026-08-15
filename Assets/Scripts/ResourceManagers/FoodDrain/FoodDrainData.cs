using System;
using UnityEngine;

[Serializable]
public class FoodDrainData
{
    public float DrainAmount = 0f;
    public float DrainTime = 0f;

    public static FoodDrainData Default()
    {
        return new FoodDrainData();
    }

    public static FoodDrainData Create(FoodDrainManager foodDrain)
    {
        return new FoodDrainData()
        {
            DrainAmount = foodDrain.DrainAmount,
            DrainTime = foodDrain.CurrentDrainTime
        };
    }
}