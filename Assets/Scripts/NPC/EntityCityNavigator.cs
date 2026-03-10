using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public Building currentBuilding { get; private set; } = null;
    public ElevatorModule CurrentElevator { get; private set; } = null;

    public Building currentPathBuilding { get; private set; } = null;
    public ElevatorModule currentPathElevator { get; private set; } = null;
    public Building targetBuilding = null;
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
                HandleStoppedFollowingPath();
                break;
            case FollowingPathState.GoingToWaiting:
                HandleStoppedGoingToWaiting();
                break;
            case FollowingPathState.GoingToRiding:
                HandleStoppedGoingToRiding();
                break;
        }
    }

    private void HandleStoppedFollowingPath()
    {
        SetState(FollowingPathState.None);
    }

    private void HandleStoppedGoingToWaiting()
    {
        SetState(FollowingPathState.Waiting);
    }

    private void HandleStoppedGoingToRiding()
    {
        SetState(FollowingPathState.Riding);
    }

    // Target Building
    public void HandleInteractBuildingSeted(Building targetBuilding)
    {
        this.targetBuilding = targetBuilding;

        if (IsRidingOnElevator) return;
         
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
        return IsRidingOnElevator && CurrentElevator.IsPossibleToExit();
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
        CurrentElevator.RemovePassenger(this);
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

        EnterBuilding(building);
    }

    public void OnStayBuildingTrigger(Building building)
    {
        if (currentBuilding) return;
        if (IsRidingOnElevator) return;

        EnterBuilding(building);
    }

    public void OnExitedBuildingTrigger(Building building)
    {
        if (building != currentBuilding) return;
        if (IsRidingOnElevator) return;

        ExitBuilding();
    }

    private void EnterBuilding(Building building)
    {
        if (building == null) {
            Debug.LogWarning("building is NULL");
            return;
        }
        if (building == currentBuilding) {
            Debug.LogWarning("building is a currentBuilding already");
            return;
        }

        currentBuilding = building;
        CurrentElevator = building.GetComponent<ElevatorModule>();
        onEnteredBuilding?.Invoke(currentBuilding);
        OnEnterBuilding();
    }

    private void OnEnterBuilding()
    {
        if (IsRidingOnElevator) return;
        if (!HasPath) return;

        if (IsOnCurrentPathBuilding()) {
            OnReachedPath();
        }
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
        if (currentBuilding == targetBuilding) {
            HandleReachedTarget();
            UpdateFollowingPathState();
        }
        else {
            AddPathIndex();
            UpdatePathBuildings();
        }
        UpdateFollowingPathState();
    }

    private bool IsOnCurrentPathBuilding()
    {
        return currentBuilding && HasPath && currentBuilding == currentPathBuilding;
    }

    private void AddPathIndex()
    {
        pathIndex++;
    }

    private void UpdatePathBuildings()
    {
        currentPathBuilding = pathBuildings[pathIndex];
        currentPathElevator = currentPathBuilding.GetComponent<ElevatorModule>();
    }

    // Follow Path
    private void UpdateFollowingPathState()
    {
        if (ShouldUseElevator()) {
            FollowingPathState state = CurrentElevator.IsPossibleToEnter() ? FollowingPathState.GoingToRiding : FollowingPathState.GoingToWaiting;
            SetState(state);
        }
        else if (currentPathBuilding) {
            SetState(FollowingPathState.FollowingPath);
        }
        else {
            SetState(FollowingPathState.None);
        }
    }

    private bool ShouldUseElevator()
    {
        if (!CurrentElevator || !currentPathElevator)
            return false;

        TowerBuilding currentTowerBuilding = CurrentElevator.GetComponent<TowerBuilding>();
        TowerBuilding currentPathTowerBuilding = currentPathElevator.GetComponent<TowerBuilding>();

        if (currentTowerBuilding.placeIndex == currentPathTowerBuilding.placeIndex)
            return true;
        else
            return false;
    }

    private void HandleReachedTarget()
    {
        SetState(FollowingPathState.None);
        onReachedTarget?.Invoke(currentBuilding);
    }

    // Elevators
    public void OnCurrentElevatorStoppedMoving()
    {
        if (ShouldFindPath()) {
            TryFindPathToTargetBuilding();
        }

        if (IsRidingOnElevator) {
            HandleElevatorStopWhileRiding();
        }
        else if (IsWaitingForElevator) {
            HandleElevatorStopWhileWaiting();
        }
    }

    private bool ShouldFindPath()
    {
        return targetBuilding && !HasPath;
    }

    private void HandleElevatorStopWhileRiding()
    {
        Debug.Log("HandleElevatorStopWhileRiding");
        if (HasPath) {
            OnReachedPath();
        }
        else {
            ExitElevatorAndMove();
        }
    }

    private void ExitElevatorAndMove()
    {
        SetState(FollowingPathState.None);
        movement.MoveTo(currentBuilding.GetInteractionTransform().position);
    }

    private void HandleElevatorStopWhileWaiting()
    {
        Debug.Log("HandleElevatorStopWhileWaiting");
        if (CurrentElevator.IsPossibleToEnter()) {
            SetState(FollowingPathState.GoingToRiding);
        }
    }

    public void OnCurrentElevatorChangedFloor()
    {
        EnterBuilding(CurrentElevator.spawnedElevatorCabin.OwnedElevator.OwnedBuilding);
    }

    public void OnElevatorMoving(Vector3 direction, float speed)
    {
        movement.Move(direction, speed);
    }

    // State
    public void SetState(FollowingPathState state)
    {
        Debug.Log(state);
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
                CurrentElevator.AddPassenger(this);
                movement.MoveTo(CurrentElevator.OwnedBuilding.GetInteractionTransform().position);
                break;
            case FollowingPathState.Waiting:
                CurrentElevator.AddPassenger(this);
                movement.StopMoving();
                break;
            case FollowingPathState.GoingToRiding:
                CurrentElevator.AddPassenger(this);
                movement.MoveTo(CurrentElevator.GetCabinRidingTransform().position);
                break;
            case FollowingPathState.Riding:
                CurrentElevator.AddPassenger(this);
                movement.StopMoving();
                movement.SetAgentEnabled(false);
                break;
        }
    }

    public void ExitState(FollowingPathState state)
    {
        switch (state) {
            case FollowingPathState.GoingToWaiting:
                CurrentElevator.RemovePassenger(this);
                break;
            case FollowingPathState.Waiting:
                CurrentElevator.RemovePassenger(this);
                break;
            case FollowingPathState.GoingToRiding:
                CurrentElevator.RemovePassenger(this);
                break;
            case FollowingPathState.Riding:
                CurrentElevator.RemovePassenger(this);
                movement.SetAgentEnabled(true);
                break;
        }
    }

    // Events
    private void OnNavMeshBaked()
    {
        if (!targetBuilding) return;

        if (!TryFindPathToTargetBuilding()) {
            SetState(FollowingPathState.None);
        }
    }

    //private void OnBuildingDemolished(Building building)
    //{
    //    if (building == targetBuilding) {
    //        HandleInteractBuildingRemoved();
    //    }
    //}
}