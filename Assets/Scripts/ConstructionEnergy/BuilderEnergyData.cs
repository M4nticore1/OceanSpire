using System;
using UnityEngine;

[Serializable]
public class BuilderEnergyData
{
    public float CurrentEnergy = 1f;
    public long? NextChargeTime = null;

    public static BuilderEnergyData Default()
    {
        return new BuilderEnergyData();
    }

    public static BuilderEnergyData Create(BuilderEnergyManager constructionEnergyManager)
    {
        return new BuilderEnergyData()
        {
            CurrentEnergy = constructionEnergyManager.CurrentEnergy,
            NextChargeTime = constructionEnergyManager.NextChargeTime
        };
    }
}