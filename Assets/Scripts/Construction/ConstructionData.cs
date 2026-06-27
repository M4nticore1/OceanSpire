using System;
using UnityEngine;

[Serializable]
public class ConstructionData
{
    public long ConstructionStartTime = 0;
    public long ConstructionFinishTime = 0;
    public bool IsUnderConstruction = false;

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
            IsUnderConstruction = construction.IsUnderConstruction
        };
    }
}
