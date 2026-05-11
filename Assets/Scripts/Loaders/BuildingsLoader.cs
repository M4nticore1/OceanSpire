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

    protected override void Load(WorldData worldData)
    {
        if (worldData?.GroundBuildings != null) {
            LoadGroundBuildings(worldData.GroundBuildings);
        }
        else {
            InitGroundBuildins();
        }

        if (worldData?.FloorFrameBuildings != null && worldData?.TowerBuildings != null) {
            ClearTowerBuildinds();
            LoadFloorFrames(worldData.FloorFrameBuildings);
            LoadTowerBuildings(worldData.TowerBuildings);
        }
        else {
            InitFloorBuildinds();
            InitTowerBuildings();
        }
    }

    private void LoadGroundBuildings(BuildingData[] buildingsData)
    {
        var buildings = BuildingsManager.Instance.GroundBuildings().ToArray();

        for (int i = 0; i < buildings.Length; i++) {
            if (buildingsData.Length <= i) return;
            var building = buildings[i];
            BuildingData buildingData;

            buildingData = buildingsData[i];

            building.Init(buildingData);
        }
    }

    private void LoadFloorFrames(TowerBuildingData[] floorFrameBuildingsData)
    {
        for (int i = 0; i < floorFrameBuildingsData.Length; i++) {
            int id = (int)BuildingIdEnum.FloorFrame;
            var prefab = BuildingsList.Instance.GetBuilding(id) as TowerBuilding;
            TowerBuildingData buildingData = TowerBuildingData.Create(prefab);
            buildingData.InstanceId = InstancesManager.Instance.GetNextInstanceId();
            buildingData.FloorIndex = i;

            BuildingFactory.CreateBuilding(prefab, buildingData);
        }
    }

    private void LoadTowerBuildings(TowerBuildingData[] towerBuildingsData)
    {
        foreach (var towerBuildingData in towerBuildingsData) {
            if (towerBuildingData == null) continue;
            if (towerBuildingData.FloorIndex >= BuildingsManager.Instance.BuiltFloors.Count) continue;
            if (towerBuildingData.PlaceIndex < 0) continue;
            if (towerBuildingData.PlaceIndex >= BuildingsManager.RoomsCountPerFloor) continue;

            var prefab = BuildingsList.Instance.GetBuilding(towerBuildingData.Id) as TowerBuilding;
            BuildingFactory.CreateBuilding(prefab, towerBuildingData);
        }
    }

    private void ClearTowerBuildinds()
    {
        for (int i = BuildingsManager.Instance.BuiltFloors.Count - 1; i >= 0; i--) {
            var floor = BuildingsManager.Instance.BuiltFloors[i];
            floor.OwnedBuilding.Demolish();
        }
    }

    private void InitGroundBuildins()
    {
        var buildings = BuildingsManager.Instance.GroundBuildings().ToArray();

        for (int i = 0; i < buildings.Length; i++) {
            var building = buildings[i];

            BuildingData buildingData = BuildingData.Create(building);
            buildingData.InstanceId = InstancesManager.Instance.GetNextInstanceId();

            building.Init(buildingData);
        }
    }

    private void InitFloorBuildinds()
    {
        int floorsCount = BuildingsManager.Instance.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            var floor = BuildingsManager.Instance.BuiltFloors[i];
            var building = floor.OwnedBuilding as TowerBuilding;

            var buildingData = TowerBuildingData.Create(building);
            buildingData.InstanceId = InstancesManager.Instance.GetNextInstanceId();
            buildingData.FloorIndex = i;

            building.Init(buildingData);
        }
    }

    private void InitTowerBuildings()
    {
        int floorsCount = BuildingsManager.Instance.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            var floor = BuildingsManager.Instance.BuiltFloors[i];

            for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                var roomPlace = floor.RoomBuildingPlaces[j];

                var room = roomPlace.PlacedBuilding;
                if (!room) continue;

                var data = TowerBuildingData.Create(room);
                data.InstanceId = InstancesManager.Instance.GetNextInstanceId();
                data.FloorIndex = i;
                data.PlaceIndex = j;

                room.Init(data);
            }
        }
    }
}