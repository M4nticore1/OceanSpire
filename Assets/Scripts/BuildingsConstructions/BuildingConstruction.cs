using NUnit.Framework;
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
    public List<GameObject> buildingDetails = new List<GameObject>();

    public List<BuildingAction> buildingInteractions = new List<BuildingAction>();

    public virtual void Build()
    {

    }
}
