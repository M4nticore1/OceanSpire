using System;
using System.Collections.Generic;
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
    [SerializeField] private int pathIndex = 0;
    private bool HasPath => pathBuildings.Count > 0;

    // Positions
    public int FloorIndex { get; private set; } = 0;
    public int PlaceIndex { get; private set; } = 0;

    // State
    public FollowingPathState FollowingPathState = FollowingPathState.None;
    public bool IsFollowingPath => FollowingPathState != FollowingPathState.None;
    public bool IsGoingToWaitingForElevator => FollowingPathState == FollowingPathState.GoingToWaiting;
    public bool IsWaitingForElevator => FollowingPathState == FollowingPathState.Waiting;
    public bool IsGoingToRidingOnElevator => FollowingPathState == FollowingPathState.GoingToRiding;
    public bool IsRidingOnElevator => FollowingPathState == FollowingPathState.Riding;

    public event Action<Building> OnEnteredBuilding;
    public event Action<Building> OnExitedBuilding;
    public event Action onReachedPath;

    private void OnEnable()
    {
        movement.OnMovementStopped += OnStoppedMoving;
        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        movement.OnMovementStopped -= OnStoppedMoving;
        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnStoppedMoving()
    {
        switch (FollowingPathState) {
            case FollowingPathState.FollowingPath:
                SetState(FollowingPathState.None);
                break;
            case FollowingPathState.GoingToWaiting:
                SetState(FollowingPathState.Waiting);
                break;
            case FollowingPathState.GoingToRiding:
                SetState(FollowingPathState.Riding);
                break;
            case FollowingPathState.ExitingElevator:
                SetState(FollowingPathState.None);
                break;
        }
    }

    // Target Building
    public void SetTargetBuilding(Building target)
    {
        TargetBuilding = target;
    }

    public bool TryFindPathToTargetBuilding()
    {
        if (!TargetBuilding) return false;

        ResetPath();

        TowerBuilding startTowerBuilding;
        BuildingPlace startPlace;

        if (IsRidingOnElevator && CurrentElevator.SpawnedElevatorCabin.IsMoving) {
            int nextFloor = CurrentElevator.SpawnedElevatorCabin.NextFloor;
            startTowerBuilding = BuildingsManager.Instance.BuiltFloors[nextFloor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
            startPlace = startTowerBuilding ? startTowerBuilding.BuildingPlace : null;
        }
        else {
            startTowerBuilding = CurrentBuilding as TowerBuilding;
            startPlace = startTowerBuilding ? startTowerBuilding.BuildingPlace : null;
        }

        if (!PathFinder.TryFindBuildingPath(startPlace, TargetBuilding, ref pathBuildings)) return false;

        SortPath(ref pathBuildings);
        UpdatePathBuildings(pathBuildings);
        FollowPath();

        return true;
    }

    public void UpdateFollowingPathState()
    {
        if (ShouldRideOnElevator()) {
            SetState(FollowingPathState.GoingToRiding);
        }
        else if (ShouldWaitForElevator()) {
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

    public void RemovePath()
    {
        ResetPath();
        UpdatePathBuildings(pathBuildings);
        SetTargetBuilding(null);
    }

    // Trigger
    public void OnEnteredBuildingTrigger(Building building)
    {
        if (building == CurrentBuilding) return;
        if (!TryEnterBuilding(building)) return;

        FollowPath();
    }

    public void OnStayBuildingTrigger(Building building)
    {
        if (CurrentBuilding) return;
        if (building == CurrentBuilding) return;
        if (!TryEnterBuilding(building)) return;

        FollowPath();
    }

    public void OnExitedBuildingTrigger(Building building)
    {
        TryExitBuilding(building);
    }

    // IElevatorPassenger
    public void OnElevatorChangedFloor(Building building)
    {
        EnterBuilding(building);
        FollowPath();
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

    // State
    public void SetState(FollowingPathState state)
    {
        ExitState(FollowingPathState);
        FollowingPathState = state;
        EnterState(FollowingPathState);
    }

    public void EnterState(FollowingPathState state)
    {
        //Debug.Log($"{gameObject} enter state {state}");
        switch (state) {
            case FollowingPathState.None:
                movement.StopMoving();
                break;
            case FollowingPathState.FollowingPath:
                movement.TryMoveTo(CurrentPathBuilding.GetInteractionTransform().position);
                break;
            case FollowingPathState.GoingToWaiting:
                CurrentElevator.AddPassenger(this);
                movement.TryMoveTo(CurrentElevator.OwnedBuilding.GetInteractionTransform().position);
                break;
            case FollowingPathState.Waiting:
                CurrentElevator.AddPassenger(this);
                break;
            case FollowingPathState.GoingToRiding:
                CurrentElevator.AddPassenger(this);
                movement.TryMoveTo(CurrentElevator.GetCabinRidingTransform().position);
                break;
            case FollowingPathState.Riding:
                CurrentElevator.AddPassenger(this);
                movement.SetAgentEnabled(false);
                transform.SetParent(CurrentElevator.SpawnedElevatorCabin.transform);
                break;
            case FollowingPathState.ExitingElevator:
                movement.TryMoveTo(CurrentElevator.OwnedBuilding.GetInteractionTransform().position);
                break;
        }
    }

    public void ExitState(FollowingPathState state)
    {
        //Debug.Log($"{gameObject} exit state {state}");
        if (CurrentElevator && (state == FollowingPathState.GoingToWaiting || state == FollowingPathState.Waiting || state == FollowingPathState.GoingToRiding || state == FollowingPathState.Riding)) {
            CurrentElevator.RemovePassenger(this);
        }

        if (state == FollowingPathState.Riding) {
            movement.SetAgentEnabled(true);
            transform.SetParent(null);
        }
    }

    // Enter/Exit Building
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
    private void SortPath(ref List<Building> pathBuildings)
    {
        SortPathBuildings(ref pathBuildings);
        SortPathElevators(ref pathBuildings);
    }

    private void SortPathBuildings(ref List<Building> pathBuildings)
    {
        for (int i = pathBuildings.Count - 2; i >= 0; i--) {
            if (pathBuildings[i].GetComponent<ElevatorModule>()) continue;

            pathBuildings.RemoveAt(i);
        }
    }

    private void SortPathElevators(ref List<Building> pathBuildings)
    {
        int length = pathBuildings.Count;

        for (int i = length - 2; i >= 0; i--) {
            var current = pathBuildings[i].GetComponent<ElevatorModule>();
            var prev = length > i + 1 ? pathBuildings[i + 1].GetComponent<ElevatorModule>() : null;
            var next = i - 1 >= 0 ? pathBuildings[i - 1].GetComponent<ElevatorModule>() : null;

            if (i == length - 2 && (!next || next.OwnedTowerBuilding.FloorIndex == current.OwnedTowerBuilding.FloorIndex))
                pathBuildings.RemoveAt(i);

            if (!next || !prev)
                continue;

            int currentPlace = current.OwnedTowerBuilding.PlaceIndex;
            int prevPlace = prev.OwnedTowerBuilding.PlaceIndex;
            int nextPlace = next.OwnedTowerBuilding.PlaceIndex;

            bool betweenVertical = nextPlace == currentPlace && prevPlace == currentPlace;
            bool betweenHorizontal = nextPlace != currentPlace && prevPlace != currentPlace;

            if (betweenVertical || betweenHorizontal) {
                pathBuildings.RemoveAt(i);
            }
        }
    }

    private void ResetPath()
    {
        pathBuildings.Clear();
        pathIndex = 0;
    }

    private void OnReachedPath()
    {
        AddPathIndex();
        UpdatePathBuildings(pathBuildings);
        UpdateFollowingPathState();
        FollowPath();
        onReachedPath?.Invoke();
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

    private void AddPathIndex()
    {
        pathIndex++;
    }

    private void UpdatePathBuildings(List<Building> pathBuildings)
    {
        LastPathBuilding = pathIndex > 0 ? pathBuildings[pathIndex - 1] : null;
        CurrentPathBuilding = pathBuildings.Count > pathIndex ? pathBuildings[pathIndex] : TargetBuilding;

        LastPathTowerBuilding = LastPathBuilding as TowerBuilding;
        CurrentPathTowerBuilding = CurrentPathBuilding as TowerBuilding;

        LastPathElevator = LastPathTowerBuilding ? LastPathTowerBuilding.GetComponent<ElevatorModule>() : null;
        CurrentPathElevator = CurrentPathTowerBuilding ? CurrentPathBuilding.GetComponent<ElevatorModule>() : null;
    }

    // Follow Path
    public void FollowPath()
    {
        if (IsOnTargetBuilding()) {
            RemovePath();
        }
        else if (IsOnCurrentPathBuilding()) {
            OnReachedPath();
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
        if (!HasPath) return false;
        if (pathIndex == 1) return false;
        if (IsRidingOnElevator && CurrentBuilding != LastPathBuilding) return false;
        if (CurrentElevator && !CurrentElevator.IsPossibleToExit()) return false;

        return true;
    }

    private bool ShouldUseElevator()
    {
        if (!HasPath) return false;
        if (!CurrentElevator) return false;
        if (!CurrentPathTowerBuilding) return false;
        if (!CurrentPathTowerBuilding.NetworkWith(CurrentTowerBuilding)) return false;

        return true;
    }

    private bool ShouldWaitForElevator()
    {
        if (IsRidingOnElevator) return false;
        if (!ShouldUseElevator()) return false;

        return true;
    }

    private bool ShouldRideOnElevator()
    {
        if (!ShouldUseElevator()) return false;
        if (!CurrentElevator.IsPossibleToEnter()) return false;

        return true;
    }

    private bool ShouldExitFromElevator()
    {
        if (HasPath) return false;
        if (!IsRidingOnElevator) return false;
        if (!CurrentElevator) return false;
        if (!CurrentElevator.IsPossibleToExit()) return false;

        return true;
    }

    // Events
    private void OnBuildingInited(Building building)
    {
        if (TryFindPathToTargetBuilding()) return;

        SetState(FollowingPathState.None);
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (TryFindPathToTargetBuilding()) return;

        SetState(FollowingPathState.None);
    }

    private void OnBuildingDemolished(Building building)
    {
        if (TryFindPathToTargetBuilding()) return;

        SetState(FollowingPathState.None);
    }
}