using System.Collections.Generic;
using UnityEngine;

public class FloorBuilding : TowerBuilding
{
    // Building Places
    public List<BuildingPlace> roomBuildingPlaces;
    public BuildingPlace hallBuildingPlace;
    public BuildingPlace floorBuildingPlace;

    public override void InitializeBuilding(BuildingPlace buildingPlace, bool requiresConstruction, int levelIndex, int interiorIndex)
    {
        base.InitializeBuilding(buildingPlace, requiresConstruction, levelIndex, interiorIndex);

        floorBuildingPlace.InitializeBuildingPlace(floorIndex + 1);
        hallBuildingPlace.InitializeBuildingPlace(floorIndex);
        for (int i = 0; i < CityManager.roomsCountPerFloor; i++)
            roomBuildingPlaces[i].InitializeBuildingPlace(floorIndex);
    }

    public void ShowBuildingPlacesByType(Building building)
    {
        BuildingData buildingData = building.BuildingData;

        bool hasPlaceAbove = false;
        bool hasPlaceBelow = false;

        int buildingHeight = buildingData.BuildingFloors;

        if (buildingData.BuildingType == BuildingType.Room)
        {
            for (int i = 0; i < roomBuildingPlaces.Count; i++)
            {
                if (roomBuildingPlaces[i].emptyBuildingPlacesAbove >= buildingHeight - 1)
                    hasPlaceAbove = true;
                if (roomBuildingPlaces[i].emptyBuildingPlacesBelow >= buildingHeight - 1)
                    hasPlaceBelow = true;

                if (!roomBuildingPlaces[i].placedBuilding)
                {
                    if (hasPlaceAbove || hasPlaceBelow)
                        roomBuildingPlaces[i].ShowBuildingPlace(BuildingPlaceState.Valid);
                    else
                        roomBuildingPlaces[i].ShowBuildingPlace(BuildingPlaceState.Invalid);

                    if (!hasPlaceAbove)
                    {
                        for (int j = 1; j <= buildingData.BuildingFloors; j++)
                        {
                            BuildingPlace currentBuildingPlace = CityManager.Instance.builtFloors[floorIndex - j].roomBuildingPlaces[i];

                            if (currentBuildingPlace.emptyBuildingPlacesAbove == buildingData.BuildingFloors - 1)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        else if (buildingData.BuildingType == BuildingType.Hall)
        {
            if (!hallBuildingPlace.placedBuilding && CityManager.Instance.currentRoomsNumberOnFloor[floorIndex] == 0)
            {
                if(hallBuildingPlace.emptyBuildingPlacesAbove >= buildingHeight - 1)
                    hasPlaceAbove = true;
                if (hallBuildingPlace.emptyBuildingPlacesBelow >= buildingHeight - 1)
                    hasPlaceBelow = true;

                if (hasPlaceAbove || hasPlaceBelow)
                    hallBuildingPlace.ShowBuildingPlace(BuildingPlaceState.Valid);
                else
                    hallBuildingPlace.ShowBuildingPlace(BuildingPlaceState.Invalid);
            }
        }
        else if (buildingData.BuildingType == BuildingType.FloorFrame)
        {
            if (!floorBuildingPlace.placedBuilding)
            {
                floorBuildingPlace.ShowBuildingPlace(BuildingPlaceState.Valid);
            }
        }
    }

    public void HideBuildingPlacesByType(BuildingType buildingType)
    {
        if (buildingType == BuildingType.Room)
        {
            for (int i = 0; i < roomBuildingPlaces.Count; i++)
            {
                roomBuildingPlaces[i].HideBuildingPlace();
            }
        }
        else if (buildingType == BuildingType.Hall)
        {
            hallBuildingPlace.HideBuildingPlace();
        }
        else if (buildingType == BuildingType.FloorFrame)
        {
            floorBuildingPlace.HideBuildingPlace();
        }
    }

    public void HideAllBuildingPlaces()
    {
        for (int i = 0; i < roomBuildingPlaces.Count; i++)
        {
            roomBuildingPlaces[i].HideBuildingPlace();
        }

        hallBuildingPlace.HideBuildingPlace();

        floorBuildingPlace.HideBuildingPlace();
    }
}
