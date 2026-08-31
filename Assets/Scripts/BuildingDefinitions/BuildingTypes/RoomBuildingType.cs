using UnityEngine;

public class RoomBuildingType : BuildingType
{
    private BuildingsManager buildingsManager => BuildingsManager.Instance;

    public RoomBuildingType(Building building) : base(building)
    {

    }

    public override bool ShouldBuild()
    {
        return true;
    }

    public override bool ShouldBuild(BuildingPlace buildingPlace)
    {
        if (!base.ShouldBuild(buildingPlace)) return false;

        if (buildingsManager == null) return false;
        var entranceBuildingPlace = buildingsManager.EntranceBuildingPlace;

        var leftPlace = buildingPlace.NeighborBuildingPlaces[Direction.Left];
        if (leftPlace != null && leftPlace.PlacedBuilding != null || leftPlace == entranceBuildingPlace) {
            return true;
        }

        var rightPlace = buildingPlace.NeighborBuildingPlaces[Direction.Right];
        if (leftPlace != null && rightPlace.PlacedBuilding != null || rightPlace == entranceBuildingPlace) {
            return true;
        }

        var definition = Building.Definition;
        if (definition == null) return false;

        if (definition.ConnectionType == ConnectionType.Vertical) {
            var upPlace = buildingPlace.NeighborBuildingPlaces[Direction.Up];
            if (upPlace != null && upPlace.PlacedBuilding != null && upPlace.PlacedBuilding.ShouldConnectTo(TowerBuilding)) {
                return true;
            }

            var downPlace = buildingPlace.NeighborBuildingPlaces[Direction.Down];
            if (downPlace != null && downPlace.PlacedBuilding != null && downPlace.PlacedBuilding.ShouldConnectTo(TowerBuilding)) {
                return true;
            }
        }

        return false;
    }
}