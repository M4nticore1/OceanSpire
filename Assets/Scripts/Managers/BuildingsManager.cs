using System.Collections.Generic;
using UnityEngine;

public class BuildingsManager : MonoBehaviour
{
    public static BuildingsManager Instance;

    [SerializeField] private List<FloorFrameModule> builtFloors = new List<FloorFrameModule>();
    public IReadOnlyList<FloorFrameModule> BuiltFloors => builtFloors;

    [SerializeField] private Transform firstFloorBuildingTransform;
    public Transform FirstFloorBuildingTransform => firstFloorBuildingTransform;

    [Header("Environment Buildings")]
    [SerializeField] private GroundBuilding towerGate;
    public GroundBuilding TowerGate => towerGate;

    [SerializeField] private GroundBuilding pierBuilding;
    public GroundBuilding PierBuilding => pierBuilding;

    [SerializeField] private int maxFloorsCount = 25;
    public int MaxFloorsCount => maxFloorsCount;

    public BuildingPlace EntranceBuildingPlace { get; private set; }

    public const int FloorHeight = 5;
    public const int FirstFloorHeight = 10;

    public const int RoomsCountPerFloor = 8;
    public const int RoomsCountPerSide = 3;
    public const int RoomsWidth = 8;
    public const int FloorWidth = 24;
    public const int FirstBuildingFloor = 0;
    public const int FirstBuildingPlace = 1;
    public float CurrentCityHeight { get; private set; } = 0;

    public List<List<ElevatorModule>> elevatorGroups { get; private set; } = new List<List<ElevatorModule>>();

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateEnterBuildingPlace();
    }

    public void RegisterFloorModule(FloorFrameModule floorModule)
    {
        if (!builtFloors.Contains(floorModule)) {
            builtFloors.Add(floorModule);
        }

        UpdateCityHeight();
    }

    public void UnregisterFloorModule(FloorFrameModule floorModule)
    {
        builtFloors.Remove(floorModule);
        UpdateCityHeight();
    }

    public FloorFrameModule GetFloorFrameBuilding(int floorIndex)
    {
        if (floorIndex < 0) return null;
        if (floorIndex >= builtFloors.Count) return null;

        return builtFloors[floorIndex];
    }

    public BuildingPlace GetRoomPlace(int floorIndex, int PlaceIndex)
    {
        if (floorIndex < 0) return null;
        if (floorIndex >= builtFloors.Count) return null;
        if (PlaceIndex < 0) return null;
        if (PlaceIndex >= RoomsCountPerFloor) return null;

        return builtFloors[floorIndex].RoomBuildingPlaces[PlaceIndex];
    }

    public IEnumerable<GroundBuilding> GerGroundBuildings()
    {
        yield return towerGate;
        yield return pierBuilding;
    }

    public List<TowerBuilding> GetTowerBuildings()
    {
        var buildings = new List<TowerBuilding>();

        foreach (var floor in BuiltFloors) {
            if (!floor) {
                Debug.LogError($"[{nameof(BuildingsManager)}] Floor is not valid!");
                continue;
            }

            foreach (var roomPlace in floor.RoomBuildingPlaces) {
                var building = roomPlace.PlacedBuilding;
                if (!building) continue;

                buildings.Add(building);
            }
        }

        return buildings;
    }

    public List<TowerBuilding> GetAvalableRaidableBuildings()
    {
        var raidableBuildings = new List<TowerBuilding>();

        foreach (var floor in BuiltFloors) {
            if (!floor) {
                Debug.LogError($"[{nameof(BuildingsManager)}] Floor is not valid!");
                continue;
            }

            foreach (var roomPlace in floor.RoomBuildingPlaces) {
                var building = roomPlace.PlacedBuilding;
                if (!building) continue;
                if (!building.CanBeRaided()) continue;

                raidableBuildings.Add(building);
            }
        }

        return raidableBuildings;
    }

    public static int GetFloorIndexByHeight(float height)
    {
        int floorIndex = (int)((height - FirstFloorHeight) / FloorHeight);
        if (floorIndex < 0) floorIndex = 0;

        return floorIndex;
    }

    private void UpdateCityHeight()
    {
        if (builtFloors.Count > 0) {
            var index = builtFloors.Count - 1;

            var floor = builtFloors[index];
            if (!floor) {
                Debug.LogError($"[{nameof(BuildingsManager)}] Floor at index {index} is not valid");
            }

            CurrentCityHeight = floor.transform.position.y + FloorHeight;
        }
        else {
            CurrentCityHeight = FirstFloorHeight;
        }
    }

    private void UpdateEnterBuildingPlace()
    {
        if (builtFloors.Count <= FirstBuildingFloor) return;

        var floor = builtFloors[FirstBuildingFloor];
        if (!floor) return;

        if (floor.RoomBuildingPlaces.Count <= FirstBuildingPlace) return;

        var place = floor.RoomBuildingPlaces[FirstBuildingPlace];
        if (!place) return;

        EntranceBuildingPlace = place;
    }
}