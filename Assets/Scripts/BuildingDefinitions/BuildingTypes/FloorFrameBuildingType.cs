using UnityEngine;

public class FloorFrameBuildingType : BuildingType
{
    private BuildingsManager buildingsManager => BuildingsManager.Instance;

    public FloorFrameBuildingType(Building building) : base(building)
    {

    }

    public override bool ShouldBuild()
    {
        if (buildingsManager == null) return false;

        var highIndex = buildingsManager.BuiltFloors.Count - 1;
        var highFloorFrame = buildingsManager.GetFloorFrameBuilding(highIndex);
        if (highFloorFrame == null) return false;

        var entrancePlace = buildingsManager.EntranceBuildingPlace;
        foreach (var roomPlace in highFloorFrame.RoomBuildingPlaces) {
            if (roomPlace == null) continue;

            var room = roomPlace.PlacedBuilding;
            if (room == null) continue;

            var elevator = room.GetComponent<ElevatorModule>();
            if (elevator != null) {
                if (PathFinder.TryFindTowerPath(entrancePlace, roomPlace, out var path)) return true;
            }
        }

        return false;
    }
}