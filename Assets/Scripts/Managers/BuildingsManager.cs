using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingsManager : MonoBehaviour
{
    public static BuildingsManager instance;

    [SerializeField] private List<FloorFrameModule> builtFloors = new List<FloorFrameModule>();
    public List<FloorFrameModule> BuiltFloors => builtFloors;

    [Header("Environment Buildings")]
    [SerializeField] private GroundBuilding towerGate;
    public GroundBuilding TowerGate => towerGate;

    [SerializeField] private GroundBuilding pierBuilding;
    public GroundBuilding PierBuilding => pierBuilding;

    [SerializeField] private int maxFloorsCount = 25;
    public int MaxFloorsCount => maxFloorsCount;

    public const int FloorHeight = 5;
    public const int FirstFloorHeight = 10;

    public const int RoomsCountPerFloor = 8;
    public const int RoomsCountPerSide = 3;
    public const int RoomsWidth = 8;
    public const int FloorWidth = 24;
    public const int FirstBuildCityBuildingPlace = 1;
    public float currentCityHeight { get; private set; } = 0;

    public List<List<ElevatorModule>> elevatorGroups { get; private set; } = new List<List<ElevatorModule>>();

    public IEnumerable<GroundBuilding> GroundBuildings()
    {
        yield return towerGate;
        yield return pierBuilding;
    }

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        EventBus.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;
        Building.onBuildingInited += OnBuildingInited;
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        EventBus.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;
        Building.onBuildingInited -= OnBuildingInited;
        Building.onBuildingDemolished -= OnBuildingDemolished;
    }

    // Utils
    public TowerBuilding GetBuildingByIndex(int floorIndex, int buildingPlaceIndex)
    {
        TowerBuilding building = null;

        bool isFloorIndexMoreMin = floorIndex >= 0;
        bool isFloorIndexLessMax = floorIndex < builtFloors.Count;
        bool isBuildingPlaceIndexMoreMin = buildingPlaceIndex >= 0;
        bool isBuildingPlaceIndexLessMax = buildingPlaceIndex < RoomsCountPerFloor;

        if (isFloorIndexMoreMin && isFloorIndexLessMax && isBuildingPlaceIndexMoreMin && isBuildingPlaceIndexLessMax) {
            building = builtFloors[floorIndex].RoomBuildingPlaces[buildingPlaceIndex].PlacedBuilding;
        }

        return building;
    }

    public static int GetFloorIndexByHeight(float height)
    {
        int floorIndex = (int)((height - FirstFloorHeight) / FloorHeight);
        if (floorIndex < 0) floorIndex = 0;

        return floorIndex;
    }

    private void OnBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        EventBus.InvokeBuildingStartPlacing(widget.buildingPrefab);
    }

    private void OnBuildingInited(Building building)
    {
        var floorFrame = building.GetComponent<FloorFrameModule>();

        if (floorFrame) {
            if (builtFloors.Count == (floorFrame.OwnedBuilding as TowerBuilding).FloorIndex) {
                builtFloors.Add(floorFrame);
            }

            UpdateEmptyBuildingPlacesCount();
            UpdateCityHeight();
        }
    }

    private void OnBuildingDemolished(Building building)
    {
        UpdateEmptyBuildingPlacesCount();
        UpdateCityHeight();
    }

    private void UpdateEmptyBuildingPlacesCount()
    {
        List<int> lastPlacedRoomsFloorIndex = new List<int>();
        for (int i = 0; i < RoomsCountPerFloor; i++)
            lastPlacedRoomsFloorIndex.Add(0);

        int lastPlacedHallFloorIndex = 0;

        for (int i = 0; i < builtFloors.Count; i++) {
            // Set room heights
            bool isRoomPlacedOnFloor = false;
            for (int j = 0; j < RoomsCountPerFloor; j++) {
                if (builtFloors[i].RoomBuildingPlaces[j].PlacedBuilding)
                    isRoomPlacedOnFloor = true;

                if (builtFloors[i].RoomBuildingPlaces[j].PlacedBuilding)
                    lastPlacedRoomsFloorIndex[j] = i;

                for (int k = lastPlacedRoomsFloorIndex[j]; k <= i; k++) {
                    builtFloors[k].RoomBuildingPlaces[j].emptyBuildingPlacesAbove = i - k;

                    if (k != lastPlacedRoomsFloorIndex[j])
                        builtFloors[k].RoomBuildingPlaces[j].emptyBuildingPlacesBelow = k - lastPlacedRoomsFloorIndex[j] - 1;
                }
            }

            // Set hall heights
            if (builtFloors[i].HallBuildingPlace.PlacedBuilding || isRoomPlacedOnFloor) {
                lastPlacedHallFloorIndex = i;
            }

            for (int k = lastPlacedHallFloorIndex; k <= i; k++) {
                builtFloors[k].HallBuildingPlace.emptyBuildingPlacesAbove = i - k;

                if (k != lastPlacedHallFloorIndex)
                    builtFloors[k].HallBuildingPlace.emptyBuildingPlacesBelow = k - lastPlacedHallFloorIndex - 1;
            }
        }
    }

    private void UpdateCityHeight()
    {
        currentCityHeight = builtFloors[builtFloors.Count - 1].transform.position.y + FloorHeight;
    }
}
