using UnityEngine;

[CreateAssetMenu(fileName = "TowerBuildingLevelData", menuName = "Buildings Level Data/Tower Building Level Data")]
public class TowerBuildingLevelData : BuildingLevelData
{
    [Header("Non Connected")]
    [SerializeField] private BuildingConstruction constructionStraight = null;
    public BuildingConstruction ConstructionStraight => constructionStraight;
    [SerializeField] private BuildingConstruction constructionCorner = null;
    public BuildingConstruction ConstructionCorner => constructionCorner;

    [Header("Horizontal")]
    [Header("Straight")]
    [SerializeField] private BuildingConstruction constructionStraightLeft;
    public BuildingConstruction ConstructionStraightLeft => constructionStraightLeft;
    [SerializeField] private BuildingConstruction constructionStraightRight;
    public BuildingConstruction ConstructionStraightRight => constructionStraightRight;
    [SerializeField] private BuildingConstruction constructionStraightLeftRight;
    public BuildingConstruction ConstructionStraightLeftRight => constructionStraightLeftRight;

    [Header("Corner")]
    [SerializeField] private BuildingConstruction constructionCornerLeft;
    public BuildingConstruction ConstructionCornerLeft => constructionCornerLeft;
    [SerializeField] private BuildingConstruction constructionCornerRight;
    public BuildingConstruction ConstructionCornerRight => constructionCornerRight;
    [SerializeField] private BuildingConstruction constructionCornerLeftRight;
    public BuildingConstruction ConstructionCornerLeftRight => constructionCornerLeftRight;

    [Header("Vertical")]
    [Header("Straight")]
    [SerializeField] private BuildingConstruction constructionStraightUp;
    public BuildingConstruction ConstructionStraightUp => constructionStraightUp;
    [SerializeField] private BuildingConstruction constructionStraightDown;
    public BuildingConstruction ConstructionStraightDown => constructionStraightDown;
    [SerializeField] private BuildingConstruction constructionStraightUpDown;
    public BuildingConstruction ConstructionStraightUpDown => constructionStraightUpDown;

    [Header("Corner")]
    [SerializeField] private BuildingConstruction constructionCornerUp;
    public BuildingConstruction ConstructionCornerUp => constructionCornerUp;
    [SerializeField] private BuildingConstruction constructionCornerDown;
    public BuildingConstruction ConstructionCornerDown => constructionCornerDown;
    [SerializeField] private BuildingConstruction constructionCornerUpDown;
    public BuildingConstruction ConstructionCornerUpDown => constructionCornerUpDown;
}
