using System;
using UnityEngine;

[Serializable]
public class EnergyDrainData
{
    public float DrainAmount = 0f;

    public static EnergyDrainData Default()
    {
        return new EnergyDrainData();
    }

    public static EnergyDrainData Create(EnergyDrainManager energyDrain)
    {
        return new EnergyDrainData()
        {
            DrainAmount = energyDrain.CurrentDrainAmount
        };
    }
}