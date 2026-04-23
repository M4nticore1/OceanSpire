using UnityEngine;

[CreateAssetMenu(fileName = "GroundBuildingLevelData", menuName = "Buildings Level Data/Ground Building Level Data")]
public class GroundBuildingLevelData : BuildingLevelData
{
    [SerializeField] private BuildingConstruction construction = null;
    public BuildingConstruction Construction => construction;

    [SerializeField] private BuildingConstruction constructionFrame = null;
    public BuildingConstruction ConstructionFrame => constructionFrame;
}
