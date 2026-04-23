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
    public Building currentBuilding = null;
    public TowerBuilding currentTowerBuilding { get; private set; } = null;
    public ElevatorModule currentElevator { get; private set; } = null;

    public Building lastPathBuilding { get; private set; } = null;
    public Building currentPathBuilding { get; private set; } = null;

    public TowerBuilding lastPathTowerBuilding { get; private set; } = null;
    public TowerBuilding currentPathTowerBuilding { get; private set; } = null;

    public ElevatorModule lastPathElevator { get; private set; } = null;
    public ElevatorModule currentPathElevator { get; private set; } = null;

    public Building targetBuilding { get; private set; } = null;
    [SerializeField] private int pathIndex = 0;
    private bool HasPath => pathBuildings.Count > 0;

    // Positions
    public int floorIndex { get; private set; } = 0;
    public int placeIndex { get; private set; } = 0;

    // State
    public FollowingPathState followingPathState { get; private set; } = FollowingPathState.None;
    public bool IsFollowingPath => followingPathState != FollowingPathState.None;
    public bool IsGoingToWaitingForElevator => followingPathState == FollowingPathState.GoingToWaiting;
    public bool IsWaitingForElevator => followingPathState == FollowingPathState.Waiting;
    public bool IsGoingToRidingOnElevator => followingPathState == FollowingPathState.GoingToRiding;
    public bool IsRidingOnElevator => followingPathState == FollowingPathState.Riding;

    public event Action<Building> onEnteredBuilding;
    public event Action<Building> onExitedBuilding;
    public event Action onReachedPath;

    private void OnEnable()
    {
        movement.onStoppedMoving += OnStoppedMoving;
        EventBus.onNavMeshBaked += OnNavMeshBaked;
    }

    private void OnDisable()
    {
        movement.onStoppedMoving -= OnStoppedMoving;
        EventBus.onNavMeshBaked -= OnNavMeshBaked;
    }

    private void OnStoppedMoving()
    {
        switch (followingPathState) {
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
        targetBuilding = target;
    }

    public void RemoveTargetBuilding()
    {
        targetBuilding = null;
        ResetPath();
    }

    public void HandleInteractBuildingRemoved()
    {
        RemovePath();
        UpdateFollowingPathState();
    }

    public bool TryFindPathToTargetBuilding()
    {
        ResetPath();

        TowerBuilding startTowerBuilding;
        BuildingPlace startPlace;
        if (IsRidingOnElevator && currentElevator.spawnedElevatorCabin.isMoving) {
            int nextFloor = currentElevator.spawnedElevatorCabin.nextFloor;
            startTowerBuilding = BuildingsManager.instance.BuiltFloors[nextFloor].RoomBuildingPlaces[placeIndex].PlacedBuilding;
            startPlace = startTowerBuilding ? startTowerBuilding.BuildingPlace : null;
        }
        else {
            startTowerBuilding = currentBuilding as TowerBuilding;
            startPlace = startTowerBuilding ? startTowerBuilding.BuildingPlace : null;
        }

        if (!PathFinder.TryGetPathToBuilding(BuildingsManager.instance, startPlace, targetBuilding, ref pathBuildings)) return false;

        SortPath();
        AssignPathBuildings();
        FollowPath();

        return true;
    }

    // Enter/Exit Building
    private bool TryEnterBuilding(Building building)
    {
        if (currentBuilding) return false;
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

    // Building
    public void OnEnteredBuildingTrigger(Building building)
    {
        TryEnterBuilding(building);
    }

    public void OnStayBuildingTrigger(Building building)
    {
        if (currentBuilding) return;
        if (building == currentBuilding) return;

        TryEnterBuilding(building);
    }

    public void OnExitedBuildingTrigger(Building building)
    {
        if (building != currentBuilding) return;

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

    private void AssignBuildings()
    {
        currentTowerBuilding = currentBuilding as TowerBuilding;
        currentElevator = currentTowerBuilding ? currentTowerBuilding?.GetComponent<ElevatorModule>() : null;
    }

    private void AssignTowerPlace()
    {
        floorIndex = currentTowerBuilding ? currentTowerBuilding.FloorIndex : 0;
        placeIndex = currentTowerBuilding ? currentTowerBuilding.PlaceIndex : 0;
    }

    private void EnterBuilding(Building building)
    {
        currentBuilding = building;
        AssignBuildings();
        AssignTowerPlace();
        FollowPath();

        building.EnterBuilding(this);
        onEnteredBuilding?.Invoke(currentBuilding);
    }

    private void ExitBuilding()
    {
        Building lastBuilding = currentBuilding;
        currentBuilding = null;

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

    private void RemovePath()
    {
        ResetPath();
        AssignPathBuildings();
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
        if (currentBuilding != currentPathBuilding) return false;

        return true;
    }

    private bool IsOnTargetBuilding()
    {
        if (currentBuilding != targetBuilding) return false;  

        return true;
    }

    private void AddPathIndex()
    {
        pathIndex++;
    }

    private void AssignPathBuildings()
    {
        lastPathBuilding = pathIndex > 0 ? pathBuildings[pathIndex - 1] : null;
        currentPathBuilding = pathBuildings.Count > pathIndex ? pathBuildings[pathIndex] : targetBuilding;

        lastPathTowerBuilding = lastPathBuilding as TowerBuilding;
        currentPathTowerBuilding = currentPathBuilding as TowerBuilding;

        lastPathElevator = lastPathTowerBuilding ? lastPathTowerBuilding.GetComponent<ElevatorModule>() : null;
        currentPathElevator = currentPathTowerBuilding ? currentPathBuilding.GetComponent<ElevatorModule>() : null;
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

    private bool ShouldIdle()
    {
        if (IsRidingOnElevator) return false;

        return true;
    }

    private bool ShouldFollowPath()
    {
        if (!HasPath) return false;
        if (currentElevator && !currentElevator.IsPossibleToExit()) return false;

        return true;
    }

    private bool ShouldUseElevator()
    {
        if (!HasPath) return false;
        if (IsRidingOnElevator) return false;
        if (!currentElevator) return false;
        if (!currentPathTowerBuilding) return false;
        if (!currentPathTowerBuilding.NetworkWith(currentTowerBuilding)) return false;

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
        if (!currentElevator.IsPossibleToEnter()) return false;

        return true;
    }

    private bool ShouldExitFromElevator()
    {
        if (HasPath) return false;
        if (!currentElevator) return false;
        if (!currentElevator.IsPossibleToExit()) return false;

        return true;
    }

    // Elevators
    public void OnElevatorMoving(Vector3 direction, float speed)
    {
        movement.Move(direction, speed);
    }

    // State
    public void SetState(FollowingPathState state)
    {
        ExitState(followingPathState);
        followingPathState = state;
        EnterState(followingPathState);
    }

    public void EnterState(FollowingPathState state)
    {
        switch (state) {
            case FollowingPathState.None:
                movement.StopMoving();
                break;
            case FollowingPathState.FollowingPath:
                movement.TryMoveTo(currentPathBuilding.GetInteractionTransform().position);
                break;
            case FollowingPathState.GoingToWaiting:
                currentElevator.AddPassenger(this);
                movement.TryMoveTo(currentElevator.OwnedBuilding.GetInteractionTransform().position);
                break;
            case FollowingPathState.Waiting:
                currentElevator.AddPassenger(this);
                //movement.StopMoving();
                break;
            case FollowingPathState.GoingToRiding:
                currentElevator.AddPassenger(this);
                movement.TryMoveTo(currentElevator.GetCabinRidingTransform().position);
                break;
            case FollowingPathState.Riding:
                currentElevator.AddPassenger(this);
                //movement.StopMoving();
                movement.SetAgentEnabled(false);
                transform.SetParent(currentElevator.spawnedElevatorCabin.transform);
                break;
            case FollowingPathState.ExitingElevator:
                movement.TryMoveTo(currentElevator.OwnedBuilding.GetInteractionTransform().position);
                break;
        }
    }

    public void ExitState(FollowingPathState state)
    {
        switch (state) {
            case FollowingPathState.GoingToWaiting:
                currentElevator.RemovePassenger(this);
                break;
            case FollowingPathState.Waiting:
                currentElevator.RemovePassenger(this);
                break;
            case FollowingPathState.GoingToRiding:
                currentElevator.RemovePassenger(this);
                break;
            case FollowingPathState.Riding:
                currentElevator.RemovePassenger(this);
                movement.SetAgentEnabled(true);
                transform.SetParent(null);
                break;
        }
    }

    // Events
    private void OnNavMeshBaked()
    {
        if (!targetBuilding) return;

        if (!TryFindPathToTargetBuilding() && !IsGoingToWaitingForElevator) {
            SetState(FollowingPathState.None);
        }
    }
}