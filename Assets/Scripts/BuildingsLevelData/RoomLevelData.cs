using UnityEngine;

[CreateAssetMenu(fileName = "RoomLevelData", menuName = "Constructions Level Data/RoomLevelData")]
public class RoomLevelData : TowerBuildingLevelData
{
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
    [SerializeField] private BuildingConstruction constructionStraightAbove;
    public BuildingConstruction ConstructionStraightAbove => constructionStraightAbove;
    [SerializeField] private BuildingConstruction constructionStraightBelow;
    public BuildingConstruction ConstructionStraightBelow => constructionStraightBelow;
    [SerializeField] private BuildingConstruction constructionStraightAboveBelow;
    public BuildingConstruction ConstructionStraightAboveBelow => constructionStraightAboveBelow;

    [Header("Corner")]
    [SerializeField] private BuildingConstruction constructionCornerAbove;
    public BuildingConstruction ConstructionCornerAbove => constructionCornerAbove;
    [SerializeField] private BuildingConstruction constructionCornerBelow;
    public BuildingConstruction ConstructionCornerBelow => constructionCornerBelow;
    [SerializeField] private BuildingConstruction constructionCornerAboveBelow;
    public BuildingConstruction ConstructionCornerAboveBelow => constructionCornerAboveBelow;
}
