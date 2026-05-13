using System;
using System.Collections.Generic;
using UnityEngine;

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
    public Building CurrentBuilding { get; private set; }
    public TowerBuilding CurrentTowerBuilding { get; private set; }
    public ElevatorModule CurrentElevator { get; private set; }

    public Building LastPathBuilding { get; private set; }
    public Building CurrentPathBuilding { get; private set; }

    public TowerBuilding LastPathTowerBuilding { get; private set; }
    public TowerBuilding CurrentPathTowerBuilding { get; private set; }

    public ElevatorModule LastPathElevator { get; private set; }
    public ElevatorModule CurrentPathElevator { get; private set; }

    public Building TargetBuilding { get; private set; }
    [SerializeField] private int pathIndex = 0;
    private bool HasPath => pathBuildings.Count > 0;

    // Positions
    public int FloorIndex { get; private set; } = 0;
    public int PlaceIndex { get; private set; } = 0;

    // State
    public FollowingPathState FollowingPathState { get; private set; } = FollowingPathState.None;
    public bool IsFollowingPath => FollowingPathState != FollowingPathState.None;
    public bool IsGoingToWaitingForElevator => FollowingPathState == FollowingPathState.GoingToWaiting;
    public bool IsWaitingForElevator => FollowingPathState == FollowingPathState.Waiting;
    public bool IsGoingToRidingOnElevator => FollowingPathState == FollowingPathState.GoingToRiding;
    public bool IsRidingOnElevator => FollowingPathState == FollowingPathState.Riding;

    public event Action<Building> onEnteredBuilding;
    public event Action<Building> onExitedBuilding;
    public event Action onReachedPath;

    private void OnEnable()
    {
        movement.onMovementStopped += OnStoppedMoving;
        Building.onBuildingInited += OnBuildingInited;
        Building.onBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        movement.onMovementStopped -= OnStoppedMoving;
        Building.onBuildingInited -= OnBuildingInited;
        Building.onBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.onBuildingDemolished -= OnBuildingDemolished;
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

    public void RemoveTargetBuilding()
    {
        TargetBuilding = null;
        ResetPath();
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

        if (!PathFinder.TryGetPathToBuilding(startPlace, TargetBuilding, ref pathBuildings)) return false;

        SortPath();
        AssignPathBuildings();
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
        AssignPathBuildings();
    }

    // Building
    public void OnStayBuildingTrigger(Building building)
    {
        if (CurrentBuilding) return;
        if (building == CurrentBuilding) return;

        TryEnterBuilding(building);
    }

    public void OnExitedBuildingTrigger(Building building)
    {
        if (building != CurrentBuilding) return;

        TryExitBuilding();
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

    // State
    public void SetState(FollowingPathState state)
    {
        ExitState(FollowingPathState);
        FollowingPathState = state;
        EnterState(FollowingPathState);
    }

    public void EnterState(FollowingPathState state)
    {
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
                //movement.StopMoving();
                break;
            case FollowingPathState.GoingToRiding:
                CurrentElevator.AddPassenger(this);
                movement.TryMoveTo(CurrentElevator.GetCabinRidingTransform().position);
                break;
            case FollowingPathState.Riding:
                CurrentElevator.AddPassenger(this);
                //movement.StopMoving();
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
        if (CurrentBuilding) return false;
        if (IsRidingOnElevator) return false;

        EnterBuilding(building);
        return true;
    }

    private bool TryExitBuilding()
    {
        if (IsRidingOnElevator) return false;

        ExitBuilding();
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

    private void EnterBuilding(Building building)
    {
        CurrentBuilding = building;
        AssignBuildings();
        AssignTowerPlace();
        FollowPath();

        building.EnterBuilding(this);
        onEnteredBuilding?.Invoke(CurrentBuilding);
    }

    private void ExitBuilding()
    {
        Building lastBuilding = CurrentBuilding;
        CurrentBuilding = null;

        AssignBuildings();
        AssignTowerPlace();
        FollowPath();

        lastBuilding.EnterBuilding(this);
        onExitedBuilding?.Invoke(lastBuilding);
    }

    // Path
    private void SortPath()
    {
        SortBuildingInPath();
        SortElevatorsInPath();
    }

    private void SortBuildingInPath()
    {
        for (int i = pathBuildings.Count - 2; i >= 0; i--) {
            if (!pathBuildings[i].GetComponent<ElevatorModule>()) {
                pathBuildings.RemoveAt(i);
            }
        }
    }

    private void SortElevatorsInPath()
    {
        for (int i = pathBuildings.Count - 2; i > 0; i--) {
            var current = pathBuildings[i] as TowerBuilding;
            var prev = pathBuildings[i + 1] as TowerBuilding;
            var next = i - 1 >= 0 ? pathBuildings[i - 1] as TowerBuilding : null;

            if (!next || !prev) continue;

            bool betweenVertical = next.PlaceIndex == current.PlaceIndex && prev.PlaceIndex == current.PlaceIndex;
            bool betweenHorizontal = next.PlaceIndex != current.PlaceIndex && prev.PlaceIndex != current.PlaceIndex;

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
        AssignPathBuildings();
        UpdateFollowingPathState();
        onReachedPath?.Invoke();
    }

    private bool IsOnCurrentPathBuilding()
    {
        if (CurrentBuilding != CurrentPathBuilding) return false;

        return true;
    }

    private bool IsOnTargetBuilding()
    {
        if (CurrentBuilding != TargetBuilding) return false;  

        return true;
    }

    private void AddPathIndex()
    {
        pathIndex++;
    }

    private void AssignPathBuildings()
    {
        LastPathBuilding = pathIndex > 0 ? pathBuildings[pathIndex - 1] : null;
        CurrentPathBuilding = pathBuildings.Count > pathIndex ? pathBuildings[pathIndex] : TargetBuilding;

        LastPathTowerBuilding = LastPathBuilding as TowerBuilding;
        CurrentPathTowerBuilding = CurrentPathBuilding as TowerBuilding;

        LastPathElevator = LastPathTowerBuilding ? LastPathTowerBuilding.GetComponent<ElevatorModule>() : null;
        CurrentPathElevator = CurrentPathTowerBuilding ? CurrentPathBuilding.GetComponent<ElevatorModule>() : null;
    }

    // Follow Path
    private void FollowPath()
    {
        if (!HasPath) return;

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
        if (CurrentElevator && !CurrentElevator.IsPossibleToExit()) return false;

        return true;
    }

    private bool ShouldUseElevator()
    {
        if (!HasPath) return false;
        if (IsRidingOnElevator) return false;
        if (!CurrentElevator) return false;
        if (!CurrentPathTowerBuilding) return false;
        if (!CurrentPathTowerBuilding.NetworkWith(CurrentTowerBuilding)) return false;

        return true;
    }

    private bool ShouldWaitForElevator()
    {
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