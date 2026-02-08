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
    private EntityMovement movement = null;

    // Path
    [SerializeField] private List<Building> pathBuildings = new List<Building>();
    public Building currentBuilding { get; private set; } = null;
    public ElevatorModule currentElevator { get; private set; } = null;
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
         movement = GetComponent<EntityMovement>();
    }

    private void OnEnable()
    {
        movement.onStoppedMoving += OnStoppedMoving;
    }

    private void OnDisable()
    {
        movement.onStoppedMoving -= OnStoppedMoving;
    }

    private void OnStoppedMoving()
    {
        switch (followingPathState) {
            case FollowingPathState.FollowingPath: HandleStoppedFollowingPath(); break;
            case FollowingPathState.GoingToWaiting: HandleStoppedGoingToWaiting(); break;
            case FollowingPathState.GoingToRiding: HandleStoppedGoingToRiding(); break;
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
    public void OnSetedInteractBuilding(Building targetBuilding)
    {
        this.targetBuilding = targetBuilding;

        if (IsRidingOnElevator) return;

        FindPathToTargetBuilding();
    }

    private void FindPathToTargetBuilding()
    {
        BuildingPlace startPlace = (currentBuilding as TowerBuilding)?.buildingPlace;

        if (!PathFinder.TryGetPathToBuilding(CityManager.Instance, startPlace, targetBuilding, ref pathBuildings)) return;

        SortPath();
        UpdatePathBuildings();

        if (IsOnCurrentPathBuilding()) {
            OnReachedPath();
        }
        else {
            UpdateFollowingPathState();
        }
    }

    public void OnRemovedInteractBuilding()
    {
        if (ShouldExitFromElevator()) {
            HandleExitFromElevator();
        }
        else if (ShouldStopUsingElevator()) {
            HandleStopUsingElevator();
        }
        else if (ShouldSimpleStopMoving()) {
            HandleSimpleStopMoving();
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

    private void HandleSimpleStopMoving()
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
        currentElevator = building.GetComponent<ElevatorModule>();
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
        int startIndex = pathBuildings.Count - 1;
        for (int i = startIndex; i >= 0; i--) {

            Building currentBuilding = pathBuildings[i];
            ElevatorModule currentElevator = currentBuilding.GetComponent<ElevatorModule>();

            if (currentElevator) {
                Building afterNextBuilding = null;
                ElevatorModule afterNextElevator = null;
                if (i - 2 >= 0) {
                    afterNextBuilding = pathBuildings[i - 2];
                    afterNextElevator = afterNextBuilding.GetComponent<ElevatorModule>();
                }

                if (afterNextElevator && (afterNextElevator.OwnedBuilding as TowerBuilding).placeIndex == (currentElevator.OwnedBuilding as TowerBuilding).placeIndex) {
                    pathBuildings.RemoveAt(i - 1);
                }
            }
            else {
                if (i == startIndex) continue;

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

    private void OnReachedPath()
    {
        if (IsAtTargetBuilding()) {
            HandleReachedTarget();
        }
        else {
            UpdatePathIndex();
            UpdatePathBuildings();
        }

        UpdateFollowingPathState();
    }

    private bool IsOnCurrentPathBuilding()
    {
        return currentBuilding && HasPath && currentBuilding == currentPathBuilding;
    }

    private void UpdatePathIndex()
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
            HandleElevatorPath();
        }
        else if (ShouldFollowPath()) {
            SetState(FollowingPathState.FollowingPath);
        }
        else {
            SetState(FollowingPathState.None);
        }
    }

    private bool IsAtTargetBuilding()
    {
        return currentBuilding == targetBuilding;
    }

    private void HandleReachedTarget()
    {
        SetState(FollowingPathState.None);
        onReachedTarget?.Invoke(currentBuilding);
    }

    private bool ShouldUseElevator()
    {
        return currentElevator != null && currentPathElevator != null;
    }

    private void HandleElevatorPath()
    {
        SetState(currentElevator.IsPossibleToEnter() ? FollowingPathState.GoingToRiding : FollowingPathState.GoingToWaiting);
    }

    private bool ShouldFollowPath()
    {
        return currentPathBuilding != null;
    }

    // Elevators
    public void OnCurrentElevatorStoppedMoving()
    {
        if (ShouldFindPath()) {
            FindPathToTargetBuilding();
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
        if (currentElevator.IsPossibleToEnter()) {
            SetState(FollowingPathState.GoingToRiding);
        }
    }

    public void OnCurrentElevatorChangedFloor()
    {
        EnterBuilding(currentElevator.spawnedElevatorCabin.OwnedElevator.OwnedBuilding);
    }

    public void OnElevatorMoving(Vector3 direction, float speed)
    {
        movement.Move(direction, speed);
    }

    // State
    public void SetState(FollowingPathState state)
    {
        if (state == followingPathState) {
            Debug.LogWarning("Current state is already a new state.");
            return;
        }

        ExitState(followingPathState);
        followingPathState = state;
        EnterState(followingPathState);
    }

    public void EnterState(FollowingPathState state)
    {
        Debug.Log(state);
        switch (state) {
            case FollowingPathState.None:
                RemovePath();
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
                break;
        }
    }
}