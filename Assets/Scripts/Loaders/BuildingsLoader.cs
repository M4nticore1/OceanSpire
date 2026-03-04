using System.Linq;
using UnityEngine;

public class BuildingsLoader : MonoBehaviour
{
    [SerializeField] private BuildingsManager buildingsManager;

    private void Start()
    {
        WorldData saveData = WorldSaveManager.Instance.currentSaveWorldData;

        LoadEnvironmentBuildings(saveData);
        LoadFloorFrames();
        LoadTowerBuildings(saveData);
    }

    private void LoadEnvironmentBuildings(WorldData data)
    {
        GroundBuildingEntry[] groundBuildingData;

        if (data != null) {
            groundBuildingData = data.groundBuildingsData;
        }
        else {
            groundBuildingData = new GroundBuildingEntry[buildingsManager.EnvironmentBuildings().Count()];
        }

        int i = 0;
        foreach (var building in buildingsManager.EnvironmentBuildings()) {
            if (i >= groundBuildingData.Length) break;

            GroundBuildingEntry groundBuildingEntry = groundBuildingData[i];
            i++;

            if (data != null && groundBuildingEntry.id != building.BuildingData.BuildingId) continue;

            building.Init(groundBuildingEntry);
        }
    }

    private void LoadFloorFrames()
    {
        int floorsCount = buildingsManager.BuiltFloors.Count;
        for (int i = 0; i < floorsCount; i++) {
            var data = new TowerBuildingEntry(i, 0);
            FloorFrameModule floor = buildingsManager.BuiltFloors[i];
            floor.OwnedBuilding.Init(data);
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
                    TowerBuilding placedBuilding = buildingsManager.BuiltFloors[data.floorIndex].HallBuildingPlace.PlacedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.id)
                        placedBuilding.Demolish();

                    if (data.id >= 0)
                        BuildingFactory.CreateBuilding(data.id, data);
                }
                else {
                    TowerBuilding placedBuilding = buildingsManager.BuiltFloors[data.floorIndex].RoomBuildingPlaces[data.placeIndex].PlacedBuilding;
                    if (placedBuilding && placedBuilding.BuildingData.BuildingId != data.id)
                        placedBuilding.Demolish();

                    if (data.id >= 0)
                        BuildingFactory.CreateBuilding(data.id, data);
                }
            }
        }
        else {
            int floorsCount = buildingsManager.BuiltFloors.Count;
            for (int i = 0; i < floorsCount; i++) {
                FloorFrameModule floor = buildingsManager.BuiltFloors[i];

                // Hall
                var hallData = new TowerBuildingEntry(i, 0);
                floor.HallBuildingPlace.PlacedBuilding?.Init(hallData);

                // Rooms
                for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                    TowerBuilding room = floor.RoomBuildingPlaces[j].PlacedBuilding;
                    if (!room) continue;

                    var roomData = new TowerBuildingEntry(i, j);
                    room.Init(roomData);
                }
            }
        }
    }
}
