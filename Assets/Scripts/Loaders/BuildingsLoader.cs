using System.Linq;
using UnityEngine;

public class BuildingsLoader : Loader
{
    public static BuildingsLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance) {
            Debug.Log("Duplicate BuildingsLoader found in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Load(WorldData data)
    {
        LoadEnvironmentBuildings(data != null ? data.GroundBuildings : null);
        LoadFloorFrames();
        LoadTowerBuildings(data != null ? data.towerBuildings : null);
    }

    private void LoadEnvironmentBuildings(BuildingData[] data)
    {
        int groundBuildingsCount = BuildingsManager.instance.GroundBuildings().Count();

        if (data == null) {
            data = new BuildingData[groundBuildingsCount];
        }

        for (int i = 0; i < groundBuildingsCount; i++) {
            if (i >= data.Length) break;

            GroundBuilding building = BuildingsManager.instance.GroundBuildings().ToArray()[i];

            var buildingData = data[i];

            if (buildingData == null) {
                buildingData = BuildingDataFactory.CreateBuildingData(building);
                buildingData.ConstructionData.SetUnderConstruction(false);
            }

            if (WorldSaveManager.Instance.currentSaveWorldData != null && buildingData.Id != building.BuildingData.BuildingId) continue;

            building.Init(buildingData);
        }
    }

    private void LoadFloorFrames()
    {
        int floorsCount = BuildingsManager.instance.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            FloorFrameModule floor = BuildingsManager.instance.BuiltFloors[i];
            TowerBuilding building = floor.OwnedBuilding as TowerBuilding;
            building.Init(BuildingDataFactory.CreateBuildingData(building, i, 0));
        }
    }

    private void LoadTowerBuildings(TowerBuildingData[] saveData)
    {
        if (saveData != null) {
            foreach (var data in saveData) {
                if (data == null) {
                    Debug.LogError($"entry was not found in towerBuildingsData");
                    continue;
                }

                if (data.PlaceIndex == 0) {
                    TowerBuilding placedBuilding = BuildingsManager.instance.BuiltFloors[data.FloorIndex].HallBuildingPlace.PlacedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.Id) {
                        placedBuilding.Demolish();
                    }

                    if (data.Id >= 0) {
                        BuildingFactory.CreateBuilding(data);
                    }
                }
                else {
                    TowerBuilding placedBuilding = BuildingsManager.instance.BuiltFloors[data.FloorIndex].RoomBuildingPlaces[data.PlaceIndex].PlacedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.Id) {
                        placedBuilding.Demolish();
                    }

                    if (data.Id >= 0) {
                        BuildingFactory.CreateBuilding(data);
                    }
                }
            }
        }
        else {
            int floorsCount = BuildingsManager.instance.BuiltFloors.Count;

            for (int i = 0; i < floorsCount; i++) {
                FloorFrameModule floor = BuildingsManager.instance.BuiltFloors[i];

                // Hall
                BuildingPlace hallPlace = floor.HallBuildingPlace;
                TowerBuilding hall = hallPlace.PlacedBuilding;

                if (hall) {
                    TowerBuildingData data = BuildingDataFactory.CreateBuildingData(hall, i, 0);
                    data.ConstructionData.SetUnderConstruction(false);

                    hall.Init(data);
                }

                // Rooms
                for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                    BuildingPlace roomPlace = floor.RoomBuildingPlaces[j];
                    TowerBuilding room = roomPlace.PlacedBuilding;

                    if (!room) continue;

                    TowerBuildingData data = BuildingDataFactory.CreateBuildingData(room, i, j);
                    data.ConstructionData.SetUnderConstruction(false);

                    room.Init(data);
                }
            }
        }
    }
}