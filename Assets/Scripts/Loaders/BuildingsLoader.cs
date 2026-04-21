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
        LoadEnvironmentBuildings(data);
        LoadFloorFrames();
        LoadTowerBuildings(data);
    }

    private void LoadEnvironmentBuildings(WorldData data)
    {
        GroundBuildingEntry[] groundBuildingData;

        if (data != null) {
            groundBuildingData = data.groundBuildingsData;
        }
        else {
            groundBuildingData = new GroundBuildingEntry[BuildingsManager.instance.EnvironmentBuildings().Count()];
        }

        int i = 0;
        foreach (var building in BuildingsManager.instance.EnvironmentBuildings()) {
            if (i >= groundBuildingData.Length) break;

            GroundBuildingEntry groundBuildingEntry = groundBuildingData[i];
            i++;

            if (data != null && groundBuildingEntry.id != building.BuildingData.BuildingId) continue;

            building.Init(groundBuildingEntry);
        }
    }

    private void LoadFloorFrames()
    {
        int floorsCount = BuildingsManager.instance.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            var data = new TowerBuildingEntry(i, 0);
            FloorFrameModule floor = BuildingsManager.instance.BuiltFloors[i];
            TowerBuilding building = floor.OwnedBuilding as TowerBuilding;
            building.Init(data);

        }
    }

    private void LoadTowerBuildings(WorldData saveData)
    {
        if (saveData != null) {
            TowerBuildingEntry[] towerBuildingsData = saveData.towerBuildingsData;

            foreach (var data in towerBuildingsData) {
                if (data == null) {
                    Debug.LogError($"entry was not found in towerBuildingsData");
                    continue;
                }

                if (data.placeIndex == 0) {
                    TowerBuilding placedBuilding = BuildingsManager.instance.BuiltFloors[data.floorIndex].HallBuildingPlace.PlacedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.id) {
                        placedBuilding.Demolish();
                    }

                    if (data.id >= 0) {
                        BuildingFactory.CreateBuilding(data.id, data);
                    }
                }
                else {
                    TowerBuilding placedBuilding = BuildingsManager.instance.BuiltFloors[data.floorIndex].RoomBuildingPlaces[data.placeIndex].PlacedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.id) {
                        placedBuilding.Demolish();
                    }

                    if (data.id >= 0) {
                        BuildingFactory.CreateBuilding(data.id, data);
                    }
                }
            }
        }
        else {
            int floorsCount = BuildingsManager.instance.BuiltFloors.Count;

            for (int i = 0; i < floorsCount; i++) {
                FloorFrameModule floor = BuildingsManager.instance.BuiltFloors[i];

                // Hall
                var hallData = new TowerBuildingEntry(i, 0);
                BuildingPlace hallPlace = floor.HallBuildingPlace;
                TowerBuilding hall = hallPlace.PlacedBuilding;

                if (hall) {
                    hall.Init(hallData);
                }

                // Rooms
                for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                    BuildingPlace roomPlace = floor.RoomBuildingPlaces[j];
                    TowerBuilding room = roomPlace.PlacedBuilding;

                    if (!room) continue;

                    var roomData = new TowerBuildingEntry(i, j);
                    room.Init(roomData);
                }
            }
        }
    }
}