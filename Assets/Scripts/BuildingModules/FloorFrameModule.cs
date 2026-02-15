using System.Collections.Generic;
using UnityEngine;

public class FloorFrameModule : BuildingModule
{
    // Building Places
    public List<BuildingPlace> roomBuildingPlaces;
    public BuildingPlace hallBuildingPlace;
    public BuildingPlace floorBuildingPlace;

    protected override void OnInit()
    {
        int floorIndex = (OwnedBuilding as TowerBuilding).floorIndex;
        floorBuildingPlace.Init(floorIndex + 1);
        hallBuildingPlace.Init(floorIndex);
        for (int i = 0; i < CityManager.roomsCountPerFloor; i++)
            roomBuildingPlaces[i].Init(floorIndex);
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
