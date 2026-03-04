using System.Collections.Generic;
using UnityEngine;

public class FloorFrameModule : BuildingModule
{
    // Building Places
    [SerializeField] private List<BuildingPlace> roomBuildingPlaces;
    public List<BuildingPlace> RoomBuildingPlaces => roomBuildingPlaces;

    [SerializeField] private BuildingPlace hallBuildingPlace;
    public BuildingPlace HallBuildingPlace => hallBuildingPlace;

    [SerializeField] private BuildingPlace floorBuildingPlace;
    public BuildingPlace FloorBuildingPlace => floorBuildingPlace;

    protected override void OnInit()
    {
        int floorIndex = (OwnedBuilding as TowerBuilding).floorIndex;
        floorBuildingPlace.Init(floorIndex + 1);
        hallBuildingPlace.Init(floorIndex);

        for (int i = 0; i < BuildingsManager.RoomsCountPerFloor; i++) {
            roomBuildingPlaces[i].Init(floorIndex);
        }
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    protected override void OnEnterBuilding(EntityCityNavigator navigator)
    {

    }

    protected override void OnExitBuilding(EntityCityNavigator navigator)
    {

    }
}
