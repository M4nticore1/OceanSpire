using System;
using UnityEngine;

[Serializable]
public class ConstructionData
{
    public float ConstructionTime = 0f;
    public bool IsUnderConstruction = false;

    public static ConstructionData Create(ConstructionComponent construction)
    {
        return new ConstructionData()
        {
            ConstructionTime = construction.CurrentConstructionTime,
            IsUnderConstruction = construction.IsUnderConstruction
        };
    }
}
