using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct BuildingAction
{
    public List<Transform> waypoints;
    public List<int> actionTimes;
}

public class BuildingConstruction : MonoBehaviour
{
    public List<BuildingAction> buildingInteractions = new List<BuildingAction>();

    public virtual void Build()
    {

    }
}
