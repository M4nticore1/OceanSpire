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
    [SerializeField] private List<GameObject> buildingInteriors = new List<GameObject>();
    public List<GameObject> BuildingInteriors => buildingInteriors;

    [SerializeField] private List<BuildingAction> buildingInteractions = new List<BuildingAction>();
    public List<BuildingAction> BuildingInteractions => buildingInteractions;

    [Header("Storage")]
    public List<Transform> collectItemPoints = new List<Transform>();

    public virtual void Build()
    {

    }
}
