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
    Riding
}

[RequireComponent(typeof(EntityMovement))]
public class EntityCityNavigator : MonoBehaviour
{
    private BuildingsManager buildingsManager;
    private EntityMovement movement;

    // Path
    [SerializeField] private List<Building> pathBuildings = new List<Building>();
    public Building currentBuilding = null;
    public ElevatorModule currentElevator { get; private set; } = null;

    public Building currentPathBuilding { get; private set; } = null;
    public ElevatorModule currentPathElevator { get; private set; } = null;
    public Building targetBuilding { get; private set; } = null;
    [SerializeField] private int pathIndex = 0;
    private bool HasPath => pathBuildings.Count > 0;

    // Positions
    public int floorIndex => ((TowerBuilding)currentBuilding).floorIndex;
    public int buildingIndex => ((TowerBuilding)currentBuilding).placeIndex;

    // State
    public FollowingPathState followingPathState = FollowingPathState.None;
    public bool IsFollowingPath => followingPathState != FollowingPathState.None;
    public bool IsGoingToWaitingForElevator => followingPathState == FollowingPathState.GoingToWaiting;
    public bool IsWaitingForElevator => followingPathState == FollowingPathState.Waiting;
    public bool IsGoingToRidingOnElevator => followingPathState == FollowingPathState.GoingToRiding;
    public bool IsRidingOnElevator => followingPathState == FollowingPathState.Riding;

    public event Action<Building> onEnteredBuilding;
    public event Action<Building> onExitedBuilding;
    public event Action<Building> onReachedTarget;

    private void Awake()
    {
        buildingsManager = FindAnyObjectByType<BuildingsManager>();
        movement = GetComponent<EntityMovement>();
    }

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
                if (HasPath)
                    SetState(FollowingPathState.Waiting);
                else
                    SetState(FollowingPathState.None);
                break;
            case FollowingPathState.GoingToRiding:
                SetState(FollowingPathState.Riding);
                break;
        }
    }

    // Target Building
    public void HandleInteractBuildingSeted(Building targetBuilding)
    {
        this.targetBuilding = targetBuilding;  
        TryFindPathToTargetBuilding();
    }

    private bool TryFindPathToTargetBuilding()
    {
        ResetPath();

        TowerBuilding currentTowerBuilding = currentBuilding as TowerBuilding;
        BuildingPlace startPlace = currentTowerBuilding ? currentTowerBuilding.buildingPlace : null;

        if (!PathFinder.TryGetPathToBuilding(buildingsManager, startPlace, targetBuilding, ref pathBuildings)) return false;

        SortPath();
        UpdatePathBuildings();

        if (IsOnCurrentPathBuilding()) {
            OnReachedPath();
        }
        else {
            UpdateFollowingPathState();
        }

        return true;
    }

    public void HandleInteractBuildingRemoved()
    {
        if (ShouldExitFromElevator()) {
            HandleExitFromElevator();
        }
        else if (ShouldStopUsingElevator()) {
            HandleStopUsingElevator();
        }
        else if (ShouldSimpleStopMoving()) {
            SimpleStopMoving();
        }
        RemovePath();
    }

    private bool ShouldExitFromElevator()
    {
        return IsRidingOnElevator && currentElevator.IsPossibleToExit();
    }

    private bool ShouldStopUsingElevator()
    {
        return IsGoingToRidingOnElevator || IsGoingToWaitingForElevator;
    }

    private bool ShouldSimpleStopMoving()
    {
        return !IsRidingOnElevator;
    }

    private void HandleExitFromElevator()
    {
        movement.SetAgentEnabled(true);
        HandleStopUsingElevator();
    }

    private void HandleStopUsingElevator()
    {
        SetState(FollowingPathState.None);
        movement.MoveTo(currentBuilding.GetInteractionTransform().position);
        currentElevator.RemovePassenger(this);
    }

    private void SimpleStopMoving()
    {
        SetState(FollowingPathState.None);
        movement.StopMoving();
    }

    // Building
    public void OnEnteredBuildingTrigger(Building building)
    {
        if (IsRidingOnElevator) return;

        SetCurrentBuilding(building);
    }

    public void OnStayBuildingTrigger(Building building)
    {
        if (currentBuilding) return;
        if (IsRidingOnElevator) return;

        SetCurrentBuilding(building);
    }

    public void OnExitedBuildingTrigger(Building building)
    {
        if (building != currentBuilding) return;
        if (IsRidingOnElevator) return;

        ExitBuilding();
    }

    public void SetCurrentBuilding(Building building)
    {
        if (currentBuilding == building)
            return;

        currentBuilding = building;
        if (currentBuilding) {
            currentElevator = building.GetComponent<ElevatorModule>();
            EnterBuilding();
        }
        else {
            currentElevator = null;
        }
    }

    private void EnterBuilding()
    {
        if (IsOnCurrentPathBuilding()) {
            OnReachedPath();
        }
        else {
            UpdateFollowingPathState();
        }
        onEnteredBuilding?.Invoke(currentBuilding);
    }

    private void ExitBuilding()
    {
        onExitedBuilding?.Invoke(currentBuilding);
        currentBuilding = null;
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

            bool betweenVertical = next.placeIndex == current.placeIndex && prev.placeIndex == current.placeIndex;
            bool betweenHorizontal = next.placeIndex != current.placeIndex && prev.placeIndex != current.placeIndex;

            if (betweenVertical || betweenHorizontal) {
                pathBuildings.RemoveAt(i);
            }
        }
    }

    private void RemovePath()
    {
        pathBuildings.Clear();
        targetBuilding = null;
        currentPathBuilding = null;
        currentPathElevator = null;
        pathIndex = 0;
    }

    private void ResetPath()
    {
        pathBuildings.Clear();
        pathIndex = 0;
    }

    private void OnReachedPath()
    {
        AddPathIndex();
        UpdatePathBuildings();
        UpdateFollowingPathState();
    }

    private bool IsOnCurrentPathBuilding()
    {
        return HasPath && currentBuilding && currentBuilding == currentPathBuilding;
    }

    private void AddPathIndex()
    {
        pathIndex++;
    }

    private void UpdatePathBuildings()
    {
        currentPathBuilding = pathBuildings.Count > pathIndex ? pathBuildings[pathIndex] : targetBuilding;
        currentPathElevator = currentPathBuilding.GetComponent<ElevatorModule>();
    }

    // Follow Path
    public void UpdateFollowingPathState()
    {
        if (!HasPath)
            return;

        if (currentElevator) {
            if (currentPathElevator) {
                if (currentPathElevator.OwnedTowerBuilding.NetworkWith(currentElevator.OwnedTowerBuilding)) {
                    if (IsRidingOnElevator && !currentElevator.spawnedElevatorCabin.CanMoveToFloor((currentPathBuilding as TowerBuilding).floorIndex)) {
                        SetState(FollowingPathState.GoingToWaiting);
                    }
                    else if (!IsRidingOnElevator) {
                        if (currentElevator.IsPossibleToEnter()) {
                            SetState(FollowingPathState.GoingToRiding);
                        }
                        else {
                            SetState(FollowingPathState.GoingToWaiting);
                        }
                    }
                }
                else {
                    if (currentPathElevator.OwnedTowerBuilding.floorIndex == currentElevator.OwnedTowerBuilding.floorIndex) {
                        SetState(FollowingPathState.FollowingPath);
                    }
                    else {
                        SetState(FollowingPathState.GoingToWaiting);
                    }
                }
            }
            else if (currentPathBuilding) {
                SetState(FollowingPathState.FollowingPath);
            }
            else {
                SetState(FollowingPathState.GoingToWaiting);
            }
        }
        else if (currentPathBuilding) {
            SetState(FollowingPathState.FollowingPath);
        }
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
                ResetPath();
                movement.StopMoving();
                break;
            case FollowingPathState.FollowingPath:
                movement.MoveTo(currentPathBuilding.GetInteractionTransform().position);
                break;
            case FollowingPathState.GoingToWaiting:
                currentElevator.AddPassenger(this);
                movement.MoveTo(currentElevator.OwnedBuilding.GetInteractionTransform().position);
                break;
            case FollowingPathState.Waiting:
                currentElevator.AddPassenger(this);
                movement.StopMoving();
                break;
            case FollowingPathState.GoingToRiding:
                currentElevator.AddPassenger(this);
                movement.MoveTo(currentElevator.GetCabinRidingTransform().position);
                break;
            case FollowingPathState.Riding:
                currentElevator.AddPassenger(this);
                movement.StopMoving();
                movement.SetAgentEnabled(false);
                transform.SetParent(currentElevator.spawnedElevatorCabin.transform);
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