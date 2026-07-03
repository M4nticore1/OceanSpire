using System;
using UnityEngine;

[Serializable]
public class ConstructionData
{
    public long? ConstructionStartTime = null;
    public long? ConstructionFinishTime = null;
    public float ConstructionTimeReduction = 0f;

    public static ConstructionData Default()
    {
        return new ConstructionData();
    }

    public static ConstructionData Create(ConstructionComponent construction)
    {
        return new ConstructionData()
        {
            ConstructionStartTime = construction.ConstructionStartTime,
            ConstructionFinishTime = construction.ConstructionFinishTime,
            ConstructionTimeReduction = construction.ConstructionTimeReduction
        };
    }
}
