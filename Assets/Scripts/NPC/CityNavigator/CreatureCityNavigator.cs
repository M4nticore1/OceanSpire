using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CreatureCityNavigator : MonoBehaviour
{
    [SerializeField] private Human human;

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
    public bool HasPath => pathBuildings.Count > 0;

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

    [field: SerializeField] public int PathProgress { get; private set; } = 0;
    [field: SerializeField] public bool IsFollowingPath { get; private set; } = false;

    // Positions
    [field: SerializeField] public int FloorIndex { get; private set; } = 0;
    [field: SerializeField] public int PlaceIndex { get; private set; } = 0;

    public event Action<Building> OnTargetBuildingSet;
    public event Action<Building> OnTargetBuildingRemoved;

    public event Action<Building> OnEnteredBuilding;
    public event Action<Building> OnExitedBuilding;

    public event Action<Building> OnReachedPathBuilding;

    private void OnEnable()
    {
        Building.OnBuildingInited += HandleBuildingInited;
        Building.OnBuildingLevelChanged += HandleBuildingConstructionFinished;
        Building.OnBuildingDemolished += HandleBuildingDemolished;
        movement.OnMovementStopped += HandleMovementStopped;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= HandleBuildingInited;
        Building.OnBuildingLevelChanged -= HandleBuildingConstructionFinished;
        Building.OnBuildingDemolished -= HandleBuildingDemolished;
        movement.OnMovementStopped -= HandleMovementStopped;
    }

    public void Init()
    {
        Init(CityNavigatorData.Default() ?? new CityNavigatorData());
    }

    public void Init(CityNavigatorData cityNavigatorData)
    {
        if (cityNavigatorData == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] City Navigator Data is not valid");
            Init();
            return;
        }

        var enteredBuildingInstanceId = cityNavigatorData.EnteredBuildingInstanceId;
        if (enteredBuildingInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(enteredBuildingInstanceId.Value);
            if (instance != null) {
                var building = instance.GetComponent<Building>();
                EnterBuilding(building);
            }
        }

        var targetBuildingInstanceId = cityNavigatorData.TargetBuildingInstanceId;
        if (targetBuildingInstanceId != null) {
            var instance = InstancesManager.Instance.GetInstance(targetBuildingInstanceId.Value);
            if (instance != null) {
                var building = instance.GetComponent<Building>();
                SetTargetBuilding(building);
                TryUpdatePathToTargetBuilding();
            }
        }

        elevatorPassenger.Init(cityNavigatorData.ElevatorPassenger);
        waypointsComponent.Init(cityNavigatorData.Waypoints);

        //FollowPath();
    }

    // Target Building
    public void SetTargetBuilding(Building target)
    {
        if (target == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] Target Building is not valid");
            return;
        }

        if (target == TargetBuilding) return;

        TargetBuilding = target;
        TargetBuilding.SpawnedConstruction.InteractionPointsHandler.AssignInteractor(this);

        RemovePath();
        TryUpdatePathToTargetBuilding();

        OnTargetBuildingSet?.Invoke(target);
    }

    public void RemoveTargetBuilding()
    {
        if (TargetBuilding == null) return;

        TargetBuilding.SpawnedConstruction.InteractionPointsHandler.RemoveInteractor(this);
        TargetBuilding = null;

        OnTargetBuildingRemoved?.Invoke(TargetBuilding);
    }

    public bool TryUpdatePathToTargetBuilding()
    {
        if (TargetBuilding == null) {
            Debug.Log($"[{nameof(CreatureCityNavigator)}] Target Building is not valid!");
            return false;
        }

        return TryUpdatePathToBuilding(TargetBuilding);
    }

    public bool TryUpdatePathToBuilding(Building building)
    {
        if (!TryFindPathToBuilding(building, out var path)) return false;

        RemovePath();
        pathBuildings = path;
        SortPath(pathBuildings);
        UpdatePathBuildings();

        return true;
    }

    public bool TryFindPathToBuilding(Building building, out List<Building> path)
    {
        TowerBuilding startTowerBuilding;
        BuildingPlace startPlace;

        if (elevatorPassenger.IsRiding && CurrentElevator.SpawnedElevatorCabin.IsMoving) {
            var nextFloor = CurrentElevator.SpawnedElevatorCabin.NextFloor;
            startTowerBuilding = BuildingsManager.Instance.BuiltFloors[nextFloor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
            startPlace = startTowerBuilding != null ? startTowerBuilding.BuildingPlace : null;
        }
        else {
            startTowerBuilding = CurrentBuilding as TowerBuilding;
            startPlace = startTowerBuilding != null ? startTowerBuilding.BuildingPlace : BuildingsManager.Instance.EntranceBuildingPlace;
        }

        return PathFinder.TryFindBuildingPath(startPlace, building, out path);
    }

    public bool CanReachTargetBuilding()
    {
        return CanReachBuilding(TargetBuilding);
    }

    public bool CanReachBuilding(Building targetBuilding)
    {
        var startPlace = CurrentTowerBuilding != null ? CurrentTowerBuilding.BuildingPlace : null;
        List<Building> path;

        return PathFinder.TryFindBuildingPath(startPlace, targetBuilding, out path);
    }

    public void RemovePathAndTargetBuilding()
    {
        RemovePath();
        UpdatePathBuildings();
        RemoveTargetBuilding();
    }

    // Trigger
    public void OnEnteredBuildingTrigger(Building building)
    {
        if (building == null) return;
        if (building == CurrentBuilding) return;

        if (TryEnterBuilding(building)) {
            UpdatePathIndex();
            UpdatePathBuildings();
            FollowPath();
        }
    }

    public void OnStayBuildingTrigger(Building building)
    {
        if (building == null) return;
        if (CurrentBuilding != null) return;
        if (building == CurrentBuilding) return;

        TryEnterBuilding(building);
    }

    public void OnExitedBuildingTrigger(Building building)
    {
        if (building == null) return;

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
        if (building == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] Building is not valid!");
            return;
        }

        CurrentBuilding = building;
        building.EnterBuilding(this);

        UpdateCurrentBuildings();
        UpdateCurrentTowerPlace();

        OnEnteredBuilding?.Invoke(CurrentBuilding);
    }

    private void ExitBuilding(Building building)
    {
        if (building == CurrentBuilding) {
            CurrentBuilding = null;
        }

        UpdateCurrentBuildings();
        UpdateCurrentTowerPlace();

        building.ExitBuilding(this);
        OnExitedBuilding?.Invoke(building);
    }

    private void UpdateCurrentBuildings()
    {
        CurrentTowerBuilding = CurrentBuilding as TowerBuilding;
        CurrentElevator = CurrentBuilding != null ? CurrentBuilding.GetComponent<ElevatorModule>() : null;
    }

    private void UpdateCurrentTowerPlace()
    {
        FloorIndex = CurrentTowerBuilding != null ? CurrentTowerBuilding.FloorIndex : 0;
        PlaceIndex = CurrentTowerBuilding != null ? CurrentTowerBuilding.PlaceIndex : 0;
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
        if (targetBuilding == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] TargetBuilding is not valid");
            return;
        }

        var currentBuilding = CurrentBuilding;
        if (currentBuilding && currentBuilding == targetBuilding) {
            var construction = currentBuilding.SpawnedConstruction;
            if (construction == null) {
                Debug.LogError($"[{nameof(CreatureCityNavigator)}] Construction is not valid");
                return;
            }

            construction.InteractionPointsHandler.AssignInteractor(this);

            var waypoint = WaypointsComponent.GetCurrentWaypoint();
            if (waypoint == null) {
                Debug.LogError($"[{nameof(CreatureCityNavigator)}] Waypoint is not valid at {CurrentPathBuilding}");
                return;
            }

            if (!movement.IsReachedPosition(waypoint.Transform.position) && movement.TargetPosition != transform.position) {
                movement.TryMoveTo(waypoint.Transform);
            }
        }
        else {
            if (CurrentPathBuilding == null) {
                Debug.LogError($"[{nameof(CreatureCityNavigator)}] Current Path Building is not valid");
                return;
            }

            var interactPoint = CurrentPathBuilding.GetInteractPoint(0);
            Debug.Log(CurrentPathBuilding);
            if (interactPoint == null) {
                Debug.LogError($"[{nameof(CreatureCityNavigator)}] Interact point is not valid at {CurrentPathBuilding}");
                Movement.TryMoveTo(CurrentPathBuilding.transform.position, false);
                return;
            }

            var waypoint = interactPoint.GetWaypoint(0);
            if (waypoint == null) {
                Debug.LogError($"[{nameof(CreatureCityNavigator)}] Waypoint is not valid at {CurrentPathBuilding}");
                Movement.TryMoveTo(CurrentPathBuilding.transform.position, false);
                return;
            }

            var transform = waypoint.Transform;
            if (transform == null) {
                Debug.LogError($"[{nameof(CreatureCityNavigator)}] Waypoint Transform is not valid at {CurrentPathBuilding}");
                Movement.TryMoveTo(CurrentPathBuilding.transform.position, false);
                return;
            }

            if (movement.TargetPosition != transform.position) {
                movement.TryMoveTo(transform.position, false);
            }
        }
    }

    private void StopFollowingPath()
    {
        Movement.TryStopMoving();

        var currentBuilding = CurrentBuilding;
        if (currentBuilding == null) return;

        var construction = currentBuilding.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] Construction is not valid");
            return;
        }

        construction.InteractionPointsHandler.RemoveInteractor(this);
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
            if (building == null) {
                Debug.Log($"[{nameof(CreatureCityNavigator)}] Building not found on path at {name}");
                continue;
            }

            if (building.GetComponent<ElevatorModule>() != null) continue;

            pathBuildings.RemoveAt(i);
        }
    }

    private void SortPathElevators(List<Building> pathBuildings)
    {
        int length = pathBuildings.Count;

        for (int i = pathBuildings.Count - 2; i >= 0; i--) {
            var current = pathBuildings[i] != null ? pathBuildings[i].GetComponent<ElevatorModule>() : null;
            var next = i - 1 >= 0 && pathBuildings[i - 1] != null ? pathBuildings[i - 1].GetComponent<ElevatorModule>() : null;
            var previous = pathBuildings.Count > i + 1 && pathBuildings[i + 1] != null ? pathBuildings[i + 1].GetComponent<ElevatorModule>() : null;

            if (current == null) continue;

            bool connectedToNext = next != null ? current.OwnedTowerBuilding.ConnectedWith(next.OwnedTowerBuilding) : false;
            bool connectedToPrevious = previous != null ? current.OwnedTowerBuilding.ConnectedWith(previous.OwnedTowerBuilding) : false;

            bool notConnected = !connectedToNext && !connectedToPrevious;
            bool fullConnected = connectedToNext && connectedToPrevious;

            if (notConnected || fullConnected) {
                pathBuildings.RemoveAt(i);
            }
        }
    }

    private void RemovePath()
    {
        pathBuildings.Clear();
        PathProgress = 0;
    }

    private void HandleReachedPathBuilding()
    {
        UpdatePathIndex();
        UpdatePathBuildings();
        UpdateFollowingPathState();

        OnReachedPathBuilding?.Invoke(CurrentBuilding);
    }

    private bool IsOnCurrentPathBuilding()
    {
        if (CurrentPathBuilding == null) return false;
        if (CurrentBuilding != CurrentPathBuilding) return false;

        return true;
    }

    private bool IsOnTargetBuilding()
    {
        if (TargetBuilding == null) return false;
        if (CurrentBuilding != TargetBuilding) return false;

        return true;
    }

    private void UpdatePathIndex()
    {
        if (CurrentBuilding != null) {
            if (pathBuildings.Contains(CurrentBuilding)) {
                PathProgress = pathBuildings.IndexOf(CurrentBuilding) + 1;
            }
            else {
                PathProgress = Mathf.Max(0, PathProgress - 1);
            }
        }
        //else {
        //    PathProgress = 0;
        //}
    }

    private void UpdatePathBuildings()
    {
        LastPathBuilding = PathProgress - 1 >= 0 && pathBuildings.Count > PathProgress - 1 ? pathBuildings[PathProgress - 1] : null;
        CurrentPathBuilding = pathBuildings.Count > PathProgress ? pathBuildings[PathProgress] : TargetBuilding;

        LastPathTowerBuilding = LastPathBuilding as TowerBuilding;
        CurrentPathTowerBuilding = CurrentPathBuilding as TowerBuilding;

        LastPathElevator = LastPathTowerBuilding != null ? LastPathTowerBuilding.GetComponent<ElevatorModule>() : null;
        CurrentPathElevator = CurrentPathTowerBuilding != null ? CurrentPathBuilding.GetComponent<ElevatorModule>() : null;
    }

    // Events
    private void HandleBuildingInited(Building building)
    {
        if (TargetBuilding == null) return;

        if (TryUpdatePathToTargetBuilding()) {
            FollowPath();
        }
        else {
            RemovePathAndTargetBuilding();
            StopFollowingPath();
        }
    }

    private void HandleBuildingConstructionFinished(Building building)
    {
        if (TargetBuilding == null) return;

        if (TryUpdatePathToTargetBuilding()) {
            FollowPath();
        }
        else {
            RemovePathAndTargetBuilding();
            StopFollowingPath();
        }
    }

    private void HandleBuildingDemolished(Building building)
    {
        if (TargetBuilding == null) return;

        if (building == TargetBuilding) {
            RemovePathAndTargetBuilding();
        }
        else {
            if (TryUpdatePathToTargetBuilding()) {
                FollowPath();
            }
            else {
                StopFollowingPath();
            }
        }
    }

    private void HandleMovementStopped()
    {
        FollowPath();
    }

    // Follow Path
    public void FollowPath()
    {
        if (!human.ShouldFollowPath()) return;

        if (IsOnTargetBuilding()) {
            UpdateFollowingPathState();
        }
        else if (IsOnCurrentPathBuilding()) {
            HandleReachedPathBuilding();
        }
        else {
            UpdateFollowingPathState();
        }
    }

    private bool ShouldIdle()
    {
        if (TargetBuilding == null) return true;
        if (!HasPath) return true;
        if (elevatorPassenger.IsRiding) return false;

        return false;
    }

    private bool ShouldFollowPath()
    {
        if (TargetBuilding == null) return false;
        if (CurrentPathBuilding == null) return false;
        if (PathProgress > pathBuildings.Count) return false;
        if (elevatorPassenger.IsRiding && CurrentPathTowerBuilding != null && CurrentPathTowerBuilding.NetworkWith(CurrentTowerBuilding)) return false;
        if (elevatorPassenger.IsRiding && CurrentElevator != null && !CurrentElevator.IsPossibleToExit()) return false;

        return true;
    }

    private bool ShouldWaitForElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (elevatorPassenger.IsRiding) return false;
        if (!elevatorPassenger.IsGoingToWaiting) return false;
        if (CurrentBuilding == null) return false;

        var construction = CurrentBuilding.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] Construction is not valid at {CurrentBuilding}!");
            return false;
        }

        var interactPoint = construction.InteractionPointsHandler.GetInteractPoint(this);
        if (interactPoint == null) return false;

        var waypoint = interactPoint.GetWaypoint(0);
        if (waypoint == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] Waypoint is not valid at {CurrentBuilding}!");
            return false;
        }

        var transform = waypoint.Transform;
        if (transform == null) {
            Debug.LogError($"[{nameof(CreatureCityNavigator)}] Waypoint's transform is not valid at {CurrentBuilding}!");
            return false;
        }

        if (!movement.IsReachedPosition(transform.position)) return false;

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
        if (transform == null) return false;

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
        if (CurrentElevator == null) return false;
        if (!CurrentElevator.IsPossibleToExit()) return false;
        if (!healthComponent.IsAlive) return false;

        if (elevatorPassenger.IsRiding) return true;
        if (elevatorPassenger.IsGoingToRiding) return true;
        if (elevatorPassenger.IsGoingToWaiting) return true;

        return false;
    }

    private bool ShouldUseElevator()
    {
        if (CurrentElevator == null) return false;
        if (CurrentPathTowerBuilding == null) return false;
        if (!CurrentPathTowerBuilding.NetworkWith(CurrentTowerBuilding)) return false;
        if (PathProgress >= pathBuildings.Count) return false;

        return true;
    }
}