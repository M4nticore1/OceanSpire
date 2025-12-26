using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingPrefabsList", menuName = "Buildings/BuildingPrefabsList")]
public class ConstructionPrefabsList : ScriptableObject
{
    [SerializeField] private List<Building> buildingPrefabs = new List<Building>();
    [SerializeField] private List<Boat> boatPrefabs = new List<Boat>();
}
