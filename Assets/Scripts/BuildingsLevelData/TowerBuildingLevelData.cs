using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerBuildingLevelData", menuName = "Constructions Level Data/TowerBuildingLevelData")]
public class TowerBuildingLevelData : ConstructionLevelData
{
    [SerializeField] private BuildingConstruction constructionStraight = null;
    public BuildingConstruction ConstructionStraight => constructionStraight;
}
