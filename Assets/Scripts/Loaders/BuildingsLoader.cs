using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingsLoader : WorldLoader
{
    public static BuildingsLoader Instance { get; private set; }

    [SerializeField] private InstancesManager instancesManager;
    [SerializeField] private BuildingsManager buildingsManager;

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

        if (worldData?.FloorFrameBuildings != null) {
            ClearTowerBuildinds();
            LoadFloorFrames(worldData.FloorFrameBuildings);
        }
        else {
            InitFloorBuildinds();
        }

        if (worldData?.TowerBuildings != null) {
            LoadTowerBuildings(worldData.TowerBuildings);
        }
        else {
            InitTowerBuildings();
        }

        if (worldData?.ElevatorCabins != null) {
            LoadElevatorCabins(worldData.ElevatorCabins);
        }
    }

    private void LoadGroundBuildings(BuildingData[] buildingsData)
    {
        var buildings = buildingsManager.GroundBuildings().ToArray();

        for (int i = 0; i < buildings.Length; i++) {
            if (buildingsData.Length <= i) return;
            var building = buildings[i];
            var buildingData = buildingsData[i];

            building.Init(buildingData);
        }
    }

    private void LoadFloorFrames(TowerBuildingData[] floorFrameBuildingsData)
    {
        for (int i = 0; i < floorFrameBuildingsData.Length; i++) {
            var floorFrameData = floorFrameBuildingsData[i];
            var prefab = BuildingsList.Instance.GetBuilding(floorFrameData.Id) as TowerBuilding;

            Transform transform;

            if (i > 0)
                transform = buildingsManager.GetFloorFrameBuilding(i - 1)?.FloorBuildingPlace.transform;
            else
                transform = buildingsManager.FirstFloorBuildingTransform;

            BuildingFactory.CreateBuilding(prefab, transform, floorFrameData);
        }
    }

    private void LoadTowerBuildings(TowerBuildingData[] towerBuildingsData)
    {
        foreach (var towerBuildingData in towerBuildingsData) {
            if (towerBuildingData == null) continue;
            if (towerBuildingData.FloorIndex >= buildingsManager.BuiltFloors.Count) continue;
            if (towerBuildingData.PlaceIndex < 0) continue;
            if (towerBuildingData.PlaceIndex >= BuildingsManager.RoomsCountPerFloor) continue;

            var prefab = BuildingsList.Instance.GetBuilding(towerBuildingData.Id) as TowerBuilding;
            if (!prefab) continue;

            var buildingPlace = buildingsManager.GetRoomPlace(towerBuildingData.FloorIndex, towerBuildingData.PlaceIndex);
            if (!buildingPlace) continue;

            if (!buildingPlace.CanPlaceBuilding(prefab)) continue;

            BuildingFactory.CreateBuilding(prefab, buildingPlace.transform, towerBuildingData);
        }
    }

    private void LoadElevatorCabins(ElevatorCabinData[] elevatorCabinsData)
    {
        foreach (var data in elevatorCabinsData) {
            if (data == null) continue;

            var instance = instancesManager.GetInstance(data.BuildingInstanceId);
            if (!instance) continue;

            var elevator = instance.GetComponent<ElevatorModule>();
            if (!elevator) continue;

            var prefab = elevator.GetCabinConstructionPrefab();
            ConstructionFactory.CreateConstruction(prefab, elevator.transform, data);
        }
    }

    private void ClearTowerBuildinds()
    {
        for (int i = buildingsManager.BuiltFloors.Count - 1; i >= 0; i--) {
            var floor = buildingsManager.BuiltFloors[i];
            floor.OwnedBuilding.Demolish();
        }
    }

    private void InitGroundBuildins()
    {
        var buildings = buildingsManager.GroundBuildings().ToArray();

        for (int i = 0; i < buildings.Length; i++) {
            var building = buildings[i];

            var buildingData = new BuildingData
            {
                Id = building.BuildingData.BuildingId,
                InstanceId = instancesManager.GetNextInstanceId(),
                Level = LevelData.Create(building.LevelComponent),
                Upgrade = UpgradeData.Create(building.UpgradeComponent),
                Construction = ConstructionData.Create(building.ConstructionComponent),
                Crafting = CraftingModuleData.Create(building.GetComponent<CraftingModule>()),
            };

            building.Init(buildingData);
        }
    }

    private void InitFloorBuildinds()
    {
        int floorsCount = buildingsManager.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            var floor = buildingsManager.BuiltFloors[i];
            var building = floor.OwnedBuilding as TowerBuilding;

            var buildingData = new TowerBuildingData
            {
                Id = building.BuildingData.BuildingId,
                InstanceId = instancesManager.GetNextInstanceId(),
                Level = LevelData.Create(building.LevelComponent),
                Upgrade = UpgradeData.Create(building.UpgradeComponent),
                Construction = ConstructionData.Create(building.ConstructionComponent),
                Crafting = CraftingModuleData.Create(building.GetComponent<CraftingModule>()),
                FloorIndex = i,
            };

            building.Init(buildingData);
        }
    }

    private void InitTowerBuildings()
    {
        int floorsCount = buildingsManager.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            var floor = buildingsManager.BuiltFloors[i];

            for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                var roomPlace = floor.RoomBuildingPlaces[j];

                var building = roomPlace.PlacedBuilding;
                if (!building) continue;

                var buildingData = new TowerBuildingData
                {
                    Id = building.BuildingData.BuildingId,
                    InstanceId = instancesManager.GetNextInstanceId(),
                    Level = LevelData.Create(building.LevelComponent),
                    Upgrade = UpgradeData.Create(building.UpgradeComponent),
                    Construction = ConstructionData.Create(building.ConstructionComponent),
                    Crafting = CraftingModuleData.Default(),
                    FloorIndex = i,
                    PlaceIndex = j
                };

                building.Init(buildingData);
            }
        }
    }
}