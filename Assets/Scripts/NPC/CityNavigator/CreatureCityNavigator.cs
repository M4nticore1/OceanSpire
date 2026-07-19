using System;
using System.Collections.Generic;
using UnityEngine;

public class CreatureCityNavigator : MonoBehaviour
{
    [SerializeField] private Movement movement;
    public Movement Movement => movement;

    [SerializeField] private ElevatorPassenger elevatorPassenger;
    public ElevatorPassenger ElevatorPassenger => elevatorPassenger;

    [SerializeField] private CreatureWaypointsComponent waypointsComponent;
    public CreatureWaypointsComponent WaypointsComponent => waypointsComponent;

    [SerializeField] private HealthComponent healthComponent;
    public HealthComponent HealthComponent => healthComponent;

    // Path
    [SerializeField] private List<Building> pathBuildings = new List<Building>();
    public IReadOnlyList<Building> PathBuildings => pathBuildings;

    [field: SerializeField] public Building CurrentBuilding { get; private set; }
    public TowerBuilding CurrentTowerBuilding { get; private set; }
    public ElevatorModule CurrentElevator { get; private set; }

    public Building LastPathBuilding { get; private set; }
    public Building CurrentPathBuilding { get; private set; }

    public TowerBuilding LastPathTowerBuilding { get; private set; }
    public TowerBuilding CurrentPathTowerBuilding { get; private set; }

    public ElevatorModule LastPathElevator { get; private set; }
    public ElevatorModule CurrentPathElevator { get; private set; }

    [field: SerializeField] public Building TargetBuilding { get; private set; }
    public int PathProgress { get; private set; } = 0;
    public bool HasPath => pathBuildings.Count > 0;
    public bool IsFollowingPath { get; private set; } = false;

    // Positions
    public int FloorIndex { get; private set; } = 0;
    public int PlaceIndex { get; private set; } = 0;

    public event Action<Building> OnEnteredBuilding;
    public event Action<Building> OnExitedBuilding;
    public event Action onReachedPathBuilding;

    private void OnEnable()
    {
        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingLevelChanged += OnBuildingConstructionFinished;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingLevelChanged -= OnBuildingConstructionFinished;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    public void Init()
    {
        Init(CityNavigatorData.Default() ?? new CityNavigatorData());
    }

    public void Init(CityNavigatorData cityNavigatorData)
    {
        if (cityNavigatorData == null) {
            Debug.LogError("cityNavigatorData is not valid", this);
            Init();
            return;
        }

        var currentBuildingInstanceId = cityNavigatorData.EnteredBuildingInstanceId;
        if (currentBuildingInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(currentBuildingInstanceId.Value);

            if (instance) {
                var building = instance.GetComponent<Building>();
                EnterBuilding(building);
            }
        }

        var targetBuildingInstanceId = cityNavigatorData.TargetBuildingInstanceId;
        if (targetBuildingInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(targetBuildingInstanceId.Value);

            if (instance) {
                var building = instance.GetComponent<Building>();
                SetTargetBuilding(building);
                TryFindPathToTargetBuilding();
                FollowPath();
            }
        }

        elevatorPassenger.Init(cityNavigatorData.ElevatorPassenger);
        waypointsComponent.Init(cityNavigatorData.Waypoints);
    }

    // Target Building
    public void SetTargetBuilding(Building target)
    {
        if (!target) {
            Debug.LogError("targetBuilding is not valid");
            return;
        }

        if (target == TargetBuilding) return;

        TargetBuilding = target;
        TargetBuilding.SpawnedConstruction.AssignInteract(this);
    }

    public void RemoveTargetBuilding()
    {
        if (!TargetBuilding) return;

        TargetBuilding.SpawnedConstruction.RemoveInteract(this);
        TargetBuilding = null;
    }

    public bool TryFindPathToTargetBuilding()
    {
        if (!TargetBuilding) {
            Debug.Log($"TargetBuilding not found at {name}");
            return false;
        }

        return TryFindPathToBuilding(TargetBuilding);
    }

    public bool TryFindPathToBuilding(Building building)
    {
        TowerBuilding startTowerBuilding;
        BuildingPlace startPlace;

        if (elevatorPassenger.IsRiding && CurrentElevator.SpawnedElevatorCabin.IsMoving) {
            int nextFloor = CurrentElevator.SpawnedElevatorCabin.NextFloor;
            startTowerBuilding = BuildingsManager.Instance.BuiltFloors[nextFloor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
            startPlace = startTowerBuilding ? startTowerBuilding.BuildingPlace : null;
        }
        else {
            startTowerBuilding = CurrentBuilding as TowerBuilding;
            startPlace = startTowerBuilding ? startTowerBuilding.BuildingPlace : BuildingsManager.Instance.EntranceBuildingPlace;
        }

        List<Building> path;
        if (!PathFinder.TryFindBuildingPath(startPlace, building, out path)) return false;

        ResetPath();
        pathBuildings = path;
        SortPath(pathBuildings);
        UpdatePathBuildings(pathBuildings);

        return true;
    }

    public bool CanReachTargetBuilding()
    {
        return CanReachBuilding(TargetBuilding);
    }

    public bool CanReachBuilding(Building targetBuilding)
    {
        BuildingPlace startPlace = CurrentTowerBuilding ? CurrentTowerBuilding.BuildingPlace : null;
        List<Building> path;

        return PathFinder.TryFindBuildingPath(startPlace, targetBuilding, out path);
    }

    public void RemovePath()
    {
        ResetPath();
        UpdatePathBuildings(pathBuildings);
        RemoveTargetBuilding();
    }

    // Trigger
    public void OnEnteredBuildingTrigger(Building building)
    {
        if (building == CurrentBuilding) return;
        if (!TryEnterBuilding(building)) return;

        //FollowPath();
    }

    public void OnStayBuildingTrigger(Building building)
    {
        if (CurrentBuilding) return;
        if (building == CurrentBuilding) return;
        if (!TryEnterBuilding(building)) return;

        //FollowPath();
    }

    public void OnExitedBuildingTrigger(Building building)
    {
        TryExitBuilding(building);
    }

    // Enter Exit Building
    public bool TryEnterBuilding(Building building)
    {
        if (elevatorPassenger.IsRiding) return false;

        EnterBuilding(building);

        return true;
    }

    public void EnterBuilding(Building building)
    {
        CurrentBuilding = building;
        AssignBuildings();
        AssignTowerPlace();

        building.EnterBuilding(this);
        OnEnteredBuilding?.Invoke(CurrentBuilding);
    }

    private void ExitBuilding(Building building)
    {
        if (building == CurrentBuilding) {
            CurrentBuilding = null;
        }

        AssignBuildings();
        AssignTowerPlace();

        building.ExitBuilding(this);
        OnExitedBuilding?.Invoke(building);
    }

    private void AssignBuildings()
    {
        CurrentTowerBuilding = CurrentBuilding as TowerBuilding;
        CurrentElevator = CurrentBuilding?.GetComponent<ElevatorModule>();
    }

    private void AssignTowerPlace()
    {
        FloorIndex = CurrentTowerBuilding ? CurrentTowerBuilding.FloorIndex : 0;
        PlaceIndex = CurrentTowerBuilding ? CurrentTowerBuilding.PlaceIndex : 0;
    }

    // State
    public void UpdateFollowingPathState()
    {
        if (ShouldRideOnElevator()) {
            elevatorPassenger.SetState(ElevatorPassengerStateEnum.Riding);
        }
        else if (ShouldGoingToRideOnElevator()) {
            elevatorPassenger.SetState(ElevatorPassengerStateEnum.GoingToRiding);
        }
        else if (ShouldWaitForElevator()) {
            elevatorPassenger.SetState(ElevatorPassengerStateEnum.Waiting);
        }
        else if (ShouldGoingToWaitingForElevator()) {
            elevatorPassenger.SetState(ElevatorPassengerStateEnum.GoingToWaiting);
        }
        else if (ShouldExitFromElevator()) {
            elevatorPassenger.SetState(ElevatorPassengerStateEnum.Exiting);
        }
        else if (ShouldFollowPath()) {
            elevatorPassenger.SetState(ElevatorPassengerStateEnum.None);
            StartFollowingPath();
        }
        else if (ShouldIdle()) {
            elevatorPassenger.SetState(ElevatorPassengerStateEnum.None);
            StopFollowingPath();
        }
    }

    private void StartFollowingPath()
    {
        var targetBuilding = TargetBuilding;
        if (!targetBuilding) {
            Debug.LogError("targetBuilding is not valid");
            return;
        }

        var currentBuilding = CurrentBuilding;
        if (currentBuilding == targetBuilding) {
            var construction = currentBuilding.SpawnedConstruction;
            if (!construction) {
                Debug.LogError("construction is not valid");
                return;
            }

            construction.AssignInteract(this);

            var waypoint = WaypointsComponent.GetCurrentWaypoint();
            if (waypoint == null) {
                Debug.LogError("waypoint is not valid", this);
                return;
            }

            Movement.TryMoveTo(waypoint.Transform);
        }
        else {
            if (!CurrentPathBuilding) {
                Debug.LogError("cityNavigator.CurrentPathBuilding is not valid");
                return;
            }

           Movement.TryMoveTo(CurrentPathBuilding.transform.position);
        }
    }

    private void StopFollowingPath()
    {
        Movement.StopMoving();

        var currentBuilding = CurrentBuilding;
        if (!currentBuilding) return;

        var construction = currentBuilding.SpawnedConstruction;
        if (!construction) {
            Debug.LogError("construction is not valid");
            return;
        }

        construction.RemoveInteract(this);
    }

    private bool TryExitBuilding(Building building)
    {
        if (elevatorPassenger.IsRiding) return false;

        ExitBuilding(building);
        return true;
    }

    // Path
    private void SortPath(List<Building> pathBuildings)
    {
        SortPathBuildings(pathBuildings);
        SortPathElevators(pathBuildings);
    }

    private void SortPathBuildings(List<Building> pathBuildings)
    {
        for (int i = pathBuildings.Count - 2; i >= 0; i--) {
            var building = pathBuildings[i];
            if (!building) {
                Debug.Log($"Building not found on path at {name}");
                continue;
            }

            if (building.GetComponent<ElevatorModule>()) continue;

            pathBuildings.RemoveAt(i);
        }
    }

    private void SortPathElevators(List<Building> pathBuildings)
    {
        int length = pathBuildings.Count;

        for (int i = pathBuildings.Count - 2; i >= 0; i--) {
            var current = pathBuildings[i] ? pathBuildings[i].GetComponent<ElevatorModule>() : null;
            var next = i - 1 >= 0 && pathBuildings[i - 1] ? pathBuildings[i - 1].GetComponent<ElevatorModule>() : null;
            var previous = pathBuildings.Count > i + 1 && pathBuildings[i + 1] ? pathBuildings[i + 1].GetComponent<ElevatorModule>() : null;

            if (!current) continue;

            bool connectedToNext = next ? current.OwnedTowerBuilding.ConnectedWith(next.OwnedTowerBuilding) : false;
            bool connectedToPrevious = previous ? current.OwnedTowerBuilding.ConnectedWith(previous.OwnedTowerBuilding) : false;

            bool notConnected = !connectedToNext && !connectedToPrevious;
            bool fullConnected = connectedToNext && connectedToPrevious;

            if (notConnected || fullConnected) {
                pathBuildings.RemoveAt(i);
            }
        }
    }

    private void ResetPath()
    {
        pathBuildings.Clear();
        PathProgress = 0;
    }

    private void OnReachedPathBuilding()
    {
        UpdatePathIndex();
        UpdatePathBuildings(pathBuildings);
        UpdateFollowingPathState();

        onReachedPathBuilding?.Invoke();
    }

    private bool IsOnCurrentPathBuilding()
    {
        if (!CurrentPathBuilding) return false;
        if (CurrentBuilding != CurrentPathBuilding) return false;

        return true;
    }

    private bool IsOnTargetBuilding()
    {
        if (!TargetBuilding) return false;
        if (CurrentBuilding != TargetBuilding) return false;  

        return true;
    }

    private void UpdatePathIndex()
    {
        PathProgress = CurrentBuilding ? pathBuildings.IndexOf(CurrentBuilding) + 1 : 0;
    }

    private void UpdatePathBuildings(List<Building> pathBuildings)
    {
        LastPathBuilding = PathProgress - 1 >= 0 && pathBuildings.Count > PathProgress - 1 ? pathBuildings[PathProgress - 1] : null;
        CurrentPathBuilding = pathBuildings.Count > PathProgress ? pathBuildings[PathProgress] : TargetBuilding;

        LastPathTowerBuilding = LastPathBuilding as TowerBuilding;
        CurrentPathTowerBuilding = CurrentPathBuilding as TowerBuilding;

        LastPathElevator = LastPathTowerBuilding ? LastPathTowerBuilding.GetComponent<ElevatorModule>() : null;
        CurrentPathElevator = CurrentPathTowerBuilding ? CurrentPathBuilding.GetComponent<ElevatorModule>() : null;
    }

    // Events
    private void OnBuildingInited(Building building)
    {
        if (!TargetBuilding) return;

        if (TryFindPathToTargetBuilding()) {
            FollowPath();
        }
        else {
            RemovePath();
            StopFollowingPath();
        }
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (!TargetBuilding) return;

        if (TryFindPathToTargetBuilding()) {
            FollowPath();
        }
        else {
            RemovePath();
            StopFollowingPath();
        }
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!TargetBuilding) return;

        if (building == TargetBuilding) {
            RemovePath();
        }
        else {
            if (TryFindPathToTargetBuilding()) {
                FollowPath();
            }
            else {
                StopFollowingPath();
            }
        }
    }

    // Follow Path
    public void FollowPath()
    {
        if (IsOnTargetBuilding()) {
            UpdateFollowingPathState();
        }
        else if (IsOnCurrentPathBuilding()) {
            OnReachedPathBuilding();
        }
        else {
            UpdateFollowingPathState();
        }
    }

    private bool ShouldIdle()
    {
        if (elevatorPassenger.IsRiding) return false;

        return true;
    }

    private bool ShouldFollowPath()
    {
        if (!TargetBuilding) return false;
        if (!CurrentPathBuilding) return false;
        if (PathProgress > pathBuildings.Count) return false;
        if (elevatorPassenger.IsRiding && CurrentPathTowerBuilding && CurrentPathTowerBuilding.NetworkWith(CurrentTowerBuilding)) return false;
        if (elevatorPassenger.IsRiding && CurrentElevator && !CurrentElevator.IsPossibleToExit()) return false;

        return true;
    }

    private bool ShouldWaitForElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (elevatorPassenger.IsRiding) return false;
        if (!elevatorPassenger.IsGoingToWaiting) return false;
        if (!CurrentBuilding) return false;

        var construction = CurrentBuilding.SpawnedConstruction;
        if (!construction) return false;

        if (!movement.IsReachedPosition(construction.GetInteraction(this).GetWaypoint(0).Transform.position)) return false;

        return true;
    }

    private bool ShouldGoingToWaitingForElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (elevatorPassenger.IsRiding) return false;
        if (elevatorPassenger.IsGoingToRiding) return false;

        return true;
    }

    private bool ShouldRideOnElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (!elevatorPassenger.IsGoingToRiding) return false;

        var transform = CurrentElevator.GetCabinRidingTransform(elevatorPassenger);
        if (!transform) return false;

        if (!movement.IsReachedPosition(transform.position)) return false;

        return true;
    }

    private bool ShouldGoingToRideOnElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (elevatorPassenger.IsRiding) return false;
        if (!CurrentElevator.IsPossibleToEnter()) return false;

        return true;
    }

    private bool ShouldExitFromElevator()
    {
        if (HasPath) return false;
        if (!CurrentElevator) return false;
        if (!CurrentElevator.IsPossibleToExit()) return false;
        if (!healthComponent.IsAlive) return false;

        if (elevatorPassenger.IsRiding) return true;
        if (elevatorPassenger.IsGoingToRiding) return true;
        if (elevatorPassenger.IsGoingToWaiting) return true;

        return false;
    }

    private bool ShouldUseElevator()
    {
        if (!CurrentElevator) return false;
        if (!CurrentPathTowerBuilding) return false;
        if (!CurrentPathTowerBuilding.NetworkWith(CurrentTowerBuilding)) return false;
        if (PathProgress >= pathBuildings.Count) return false;

        return true;
    }
}