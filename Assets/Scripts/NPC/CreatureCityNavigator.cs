using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum FollowingPathState
{
    None,
    FollowingPath,
    GoingToWaiting,
    Waiting,
    GoingToRiding,
    Riding,
    ExitingElevator
}

public class CreatureCityNavigator : MonoBehaviour, IElevatorPassenger
{
    [SerializeField] private Movement movement;

    // Path
    [SerializeField] private List<Building> pathBuildings = new List<Building>();
    public IReadOnlyList<Building> PathBuildings => pathBuildings;

    public Building CurrentBuilding;
    public TowerBuilding CurrentTowerBuilding { get; private set; }
    public ElevatorModule CurrentElevator { get; private set; }

    public Building LastPathBuilding { get; private set; }
    public Building CurrentPathBuilding { get; private set; }

    public TowerBuilding LastPathTowerBuilding { get; private set; }
    public TowerBuilding CurrentPathTowerBuilding { get; private set; }

    public ElevatorModule LastPathElevator { get; private set; }
    public ElevatorModule CurrentPathElevator { get; private set; }

    public Building TargetBuilding;
    public int PathProgress = 0;
    private bool HasPath => pathBuildings.Count > 0;

    // Positions
    public int FloorIndex { get; private set; } = 0;
    public int PlaceIndex { get; private set; } = 0;

    // State
    public FollowingPathState CurrentState = FollowingPathState.None;
    public bool IsFollowingPath => CurrentState == FollowingPathState.FollowingPath;
    public bool IsGoingToWaitingForElevator => CurrentState == FollowingPathState.GoingToWaiting;
    public bool IsWaitingForElevator => CurrentState == FollowingPathState.Waiting;
    public bool IsGoingToRidingOnElevator => CurrentState == FollowingPathState.GoingToRiding;
    public bool IsRidingOnElevator => CurrentState == FollowingPathState.Riding;

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

    // Target Building
    public void SetTargetBuilding(Building target)
    {
        TargetBuilding = target;
    }

    public void RemoveTargetBuilding()
    {
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

        if (IsRidingOnElevator && CurrentElevator.SpawnedElevatorCabin.IsMoving) {
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

    // IElevatorPassenger
    public void OnElevatorChangedFloor(Building building)
    {
        EnterBuilding(building);
    }

    public void OnElevatorStopped()
    {
        FollowPath();
    }

    // Elevators
    public void OnElevatorMoving(Vector3 direction, float speed)
    {
        movement.Move(direction, speed);
    }

    // Enter Exit Building
    public bool TryEnterBuilding(Building building)
    {
        if (IsRidingOnElevator) return false;

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

    // State
    public void UpdateFollowingPathState()
    {
        if (ShouldRideOnElevator()) {
            SetState(FollowingPathState.Riding);
        }
        else if (ShouldGoingToRideOnElevator()) {
            SetState(FollowingPathState.GoingToRiding);
        }
        else if (ShouldWaitForElevator()) {
            SetState(FollowingPathState.Waiting);
        }
        else if (ShouldGoingToWaitingForElevator()) {
            SetState(FollowingPathState.GoingToWaiting);
        }
        else if (ShouldExitFromElevator()) {
            SetState(FollowingPathState.ExitingElevator);
        }
        else if (ShouldFollowPath()) {
            SetState(FollowingPathState.FollowingPath);
        }
        else if (ShouldIdle()) {
            SetState(FollowingPathState.None);
        }
    }

    public void SetState(FollowingPathState state)
    {
        ExitState(CurrentState);
        CurrentState = state;
        EnterState(CurrentState);
    }

    public void EnterState(FollowingPathState state)
    {
        switch (state) {
            case FollowingPathState.None:
                movement.TryStopMoving();
                break;
            case FollowingPathState.FollowingPath:
                if (CurrentBuilding == TargetBuilding) {
                    if (movement.IsDestinationReached()) break;

                    movement.TryMoveTo(CurrentPathBuilding.GetInteractionTransform(this));
                }
                else {
                    movement.TryMoveTo(CurrentPathBuilding.transform.position);
                }
                break;
            case FollowingPathState.GoingToWaiting:
                CurrentBuilding.AssignInteractTransform(this);
                CurrentElevator.AddPassenger(this);
                movement.TryMoveTo(CurrentElevator.OwnedBuilding.GetInteractionTransform(this));
                break;
            case FollowingPathState.Waiting:
                CurrentElevator.AddPassenger(this);
                break;
            case FollowingPathState.GoingToRiding:
                CurrentElevator.AddPassenger(this);
                movement.TryMoveTo(CurrentElevator.GetCabinRidingTransform());
                break;
            case FollowingPathState.Riding:
                CurrentElevator.AddPassenger(this);
                movement.SetAgentEnabled(false);
                transform.SetParent(CurrentElevator.SpawnedElevatorCabin.transform);
                break;
            case FollowingPathState.ExitingElevator:
                CurrentBuilding.AssignInteractTransform(this);
                movement.TryMoveTo(CurrentElevator.OwnedBuilding.GetInteractionTransform(this));
                break;
        }
    }

    public void ExitState(FollowingPathState state)
    {
        //Debug.Log($"{gameObject} exit state {state}");
        if (CurrentElevator && (state == FollowingPathState.GoingToWaiting || state == FollowingPathState.Waiting || state == FollowingPathState.GoingToRiding || state == FollowingPathState.Riding)) {
            CurrentElevator.RemovePassenger(this);
        }

        switch (state) {
            case FollowingPathState.GoingToWaiting:
                CurrentBuilding.TryRemoveInteractTransform(this);
                break;
            case FollowingPathState.Riding:
                movement.SetAgentEnabled(true);
                transform.SetParent(null);
                break;
            case FollowingPathState.ExitingElevator:
                CurrentBuilding.TryRemoveInteractTransform(this);
                break;
        }
    }

    private bool TryExitBuilding(Building building)
    {
        if (IsRidingOnElevator) return false;

        ExitBuilding(building);
        return true;
    }

    private void AssignBuildings()
    {
        CurrentTowerBuilding = CurrentBuilding as TowerBuilding;
        CurrentElevator = CurrentTowerBuilding ? CurrentTowerBuilding?.GetComponent<ElevatorModule>() : null;
    }

    private void AssignTowerPlace()
    {
        FloorIndex = CurrentTowerBuilding ? CurrentTowerBuilding.FloorIndex : 0;
        PlaceIndex = CurrentTowerBuilding ? CurrentTowerBuilding.PlaceIndex : 0;
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
            SetState(FollowingPathState.None);
        }
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (!TargetBuilding) return;

        if (TryFindPathToTargetBuilding()) {
            FollowPath();
        }
        else {
            SetState(FollowingPathState.None);
        }
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!TargetBuilding) return;

        if (TryFindPathToTargetBuilding()) {
            FollowPath();
        }
        else {
            SetState(FollowingPathState.None);
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
        if (IsRidingOnElevator) return false;

        return true;
    }

    private bool ShouldFollowPath()
    {
        if (!TargetBuilding) return false;
        if (!CurrentPathBuilding) return false;
        if (PathProgress > pathBuildings.Count) return false;
        if (IsRidingOnElevator && CurrentPathTowerBuilding && CurrentPathTowerBuilding.FloorIndex != CurrentTowerBuilding.FloorIndex) return false;
        if (IsRidingOnElevator && CurrentElevator && !CurrentElevator.IsPossibleToExit()) return false;

        return true;
    }

    private bool ShouldWaitForElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (IsRidingOnElevator) return false;
        if (!movement.IsReachedPosition(CurrentTowerBuilding.GetInteractionTransform(this).position)) return false;

        return true;
    }

    private bool ShouldGoingToWaitingForElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (IsRidingOnElevator) return false;
        if (IsGoingToRidingOnElevator) return false;

        return true;
    }

    private bool ShouldRideOnElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (!IsGoingToRidingOnElevator) return false;
        if (!CurrentElevator.IsPossibleToEnter()) return false;
        if (movement.IsMoving) return false;

        return true;
    }

    private bool ShouldGoingToRideOnElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (IsRidingOnElevator) return false;
        if (!CurrentElevator.IsPossibleToEnter()) return false;

        return true;
    }

    private bool ShouldExitFromElevator()
    {
        if (HasPath) return false;
        if (!CurrentElevator) return false;
        if (!CurrentElevator.IsPossibleToExit()) return false;

        if (IsRidingOnElevator) return true;
        if (IsGoingToRidingOnElevator) return true;
        if (IsGoingToWaitingForElevator) return true;

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