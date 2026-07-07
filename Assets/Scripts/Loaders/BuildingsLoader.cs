using System.Linq;
using UnityEngine;

public class BuildingsLoader : WorldLoader
{
    public static BuildingsLoader Instance { get; private set; }

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
        // 1. Загрузка или инициализация наземных зданий
        if (worldData?.GroundBuildings != null) {
            LoadGroundBuildings(worldData.GroundBuildings);
        }
        else {
            InitGroundBuildings();
        }

        // 2. Очистка и загрузка каркасов башни
        if (worldData?.FloorFrameBuildings != null) {
            ClearTowerBuildings();
            LoadFloorFrames(worldData.FloorFrameBuildings);
        }
        else {
            InitFloorBuildings();
        }

        // КРИТИЧЕСКИЙ СДВИГ: Убедитесь, что BuildingsManager принудительно обновил 
        // свои списки этажей после вызова LoadFloorFrames, иначе последующие методы не увидят этажи!
        // Пример: buildingsManager.RefreshFloorsList();

        // 3. Загрузка внутренних комнат башни
        if (worldData?.TowerBuildings != null) {
            LoadTowerBuildings(worldData.TowerBuildings);
        }
        else {
            InitTowerBuildings();
        }

        // 4. Загрузка лифтов (зависит от уже созданных зданий)
        if (worldData?.ElevatorCabins != null) {
            LoadElevatorCabins(worldData.ElevatorCabins);
        }
    }

    private void LoadGroundBuildings(BuildingData[] buildingsData)
    {
        var buildings = buildingsManager.GerGroundBuildings();
        if (buildings == null) return;

        int index = 0;
        foreach (var building in buildings) {
            if (building == null) continue;

            // Если в сохранении меньше зданий, чем на сцене, просто выходим из цикла, не прерывая весь метод Load!
            if (index >= buildingsData.Length) break;

            var buildingData = buildingsData[index];
            if (buildingData != null) {
                building.Init(buildingData);
            }
            index++;
        }
    }

    private void LoadFloorFrames(TowerBuildingData[] floorFrameBuildingsData)
    {
        for (int i = 0; i < floorFrameBuildingsData.Length; i++) {
            var floorFrameData = floorFrameBuildingsData[i];
            if (floorFrameData == null) continue;

            var prefab = BuildingsList.Instance.GetBuilding(floorFrameData.Id) as TowerBuilding;
            if (prefab == null) {
                Debug.LogError($"TowerBuilding prefab with ID {floorFrameData.Id} not found!");
                continue;
            }

            Transform spawnTransform;

            if (i > 0) {
                var previousFloor = buildingsManager.GetFloorFrameBuilding(i - 1);
                if (previousFloor == null || previousFloor.FloorBuildingPlace == null) {
                    Debug.LogError($"Previous floor frame at index {i - 1} is missing placement place!");
                    continue;
                }
                spawnTransform = previousFloor.FloorBuildingPlace.transform;
            }
            else {
                spawnTransform = buildingsManager.FirstFloorBuildingTransform;
            }

            if (spawnTransform != null) {
                BuildingFactory.CreateBuilding(prefab, spawnTransform, floorFrameData);
            }
        }
    }

    private void LoadTowerBuildings(TowerBuildingData[] towerBuildingsData)
    {
        foreach (var towerBuildingData in towerBuildingsData) {
            if (towerBuildingData == null) continue;

            if (buildingsManager.BuiltFloors == null || towerBuildingData.FloorIndex >= buildingsManager.BuiltFloors.Count) {
                Debug.LogWarning($"Floor index {towerBuildingData.FloorIndex} from save data is out of built floors bounds.");
                continue;
            }

            if (towerBuildingData.PlaceIndex < 0 || towerBuildingData.PlaceIndex >= BuildingsManager.RoomsCountPerFloor) continue;

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

            var instance = InstancesManager.Instance.GetInstance(data.OwnedBuildingInstanceId);
            if (!instance) continue;

            var elevator = instance.GetComponent<ElevatorModule>();
            if (!elevator) continue;

            var prefab = elevator.GetCabinConstructionPrefab();
            if (prefab != null) {
                ConstructionFactory.CreateConstruction(prefab, elevator.transform, data);
            }
        }
    }

    private void ClearTowerBuildings()
    {
        if (buildingsManager.BuiltFloors == null) return;

        for (int i = buildingsManager.BuiltFloors.Count - 1; i >= 0; i--) {
            var floor = buildingsManager.BuiltFloors[i];
            if (floor != null && floor.OwnedBuilding != null) {
                floor.OwnedBuilding.Demolish();
            }
        }
    }

    private void InitGroundBuildings()
    {
        var buildings = buildingsManager.GerGroundBuildings();
        if (buildings == null) return;

        foreach (var building in buildings) {
            if (building == null || building.BuildingData == null || building.LevelComponent == null) continue;

            var buildingData = new BuildingData
            {
                Id = building.BuildingData.BuildingId,
                Level = building.LevelComponent.Level,
                Upgrade = UpgradeData.Default(),
                Construction = ConstructionData.Default(),
                Crafting = CraftingModuleData.Default(),
            };

            building.Init(buildingData);
        }
    }

    private void InitFloorBuildings()
    {
        if (buildingsManager.BuiltFloors == null) return;
        int floorsCount = buildingsManager.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            var floor = buildingsManager.BuiltFloors[i];
            if (floor == null || floor.OwnedBuilding == null) continue;

            var building = floor.OwnedBuilding as TowerBuilding;
            if (building == null || building.BuildingData == null || building.LevelComponent == null) continue;

            var buildingData = new TowerBuildingData
            {
                Id = building.BuildingData.BuildingId,
                Level = building.LevelComponent.Level,
                Upgrade = UpgradeData.Default(),
                Construction = ConstructionData.Default(),
                Crafting = CraftingModuleData.Default(),
                FloorIndex = i,
            };

            building.Init(buildingData);
        }
    }

    private void InitTowerBuildings()
    {
        if (buildingsManager.BuiltFloors == null) return;
        int floorsCount = buildingsManager.BuiltFloors.Count;

        for (int i = 0; i < floorsCount; i++) {
            var floor = buildingsManager.BuiltFloors[i];
            if (floor == null || floor.RoomBuildingPlaces == null) continue;

            for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                if (j >= floor.RoomBuildingPlaces.Count) break;

                var roomPlace = floor.RoomBuildingPlaces[j];
                if (roomPlace == null) continue;

                var building = roomPlace.PlacedBuilding;
                if (!building || building.BuildingData == null || building.LevelComponent == null) continue;

                var buildingData = new TowerBuildingData
                {
                    Id = building.BuildingData.BuildingId,
                    Level = building.LevelComponent.Level,
                    Upgrade = UpgradeData.Default(),
                    Construction = ConstructionData.Default(),
                    Crafting = CraftingModuleData.Default(),
                    FloorIndex = i,
                    PlaceIndex = j
                };

                building.Init(buildingData);
            }
        }
    }
}