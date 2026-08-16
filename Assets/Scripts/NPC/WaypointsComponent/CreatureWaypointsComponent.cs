using System;
using UnityEngine;

public class CreatureWaypointsComponent : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Human human;
    [SerializeField] private Movement movement;
    [SerializeField] private CreatureCityNavigator cityNavigator;
    [SerializeField] private CreatureInteractComponent interactComponent;

    [Header("Runtime")]
    [field: SerializeField] public int CurrentWaypointIndex { get; private set; } = 0;
    [field: SerializeField] public float CurrentWaypointTime { get; private set; } = 0f;

    private CreatureWaypointsManager creatureWaypointsManager => CreatureWaypointsManager.Instance;

    private void OnEnable()
    {
        if (interactComponent != null) {
            interactComponent.OnInteractionStarted += HandleInteractionStarted;
            interactComponent.OnInteractionStopped += HandleInteractionStopped;
        }
        else {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Interact Component is not assigned on {name}!");
        }

        if (creatureWaypointsManager != null) {
            creatureWaypointsManager.RegisterComponent(this);
        }
    }

    private void OnDisable()
    {
        if (interactComponent != null) {
            interactComponent.OnInteractionStarted -= HandleInteractionStarted;
            interactComponent.OnInteractionStopped -= HandleInteractionStopped;
        }

        if (creatureWaypointsManager != null) {
            creatureWaypointsManager.UnregisterComponent(this);
        }
    }

    public void Init()
    {
        Init(CreatureWaypointsComponentData.Default());
    }

    public void Init(CreatureWaypointsComponentData waypointsData)
    {
        if (waypointsData == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Waypoints Data is not valid!");
            waypointsData = CreatureWaypointsComponentData.Default();
        }

        CurrentWaypointIndex = waypointsData.CurrentWaypointIndex;
        CurrentWaypointTime = waypointsData.CurrentWaypointTime;
    }

    public void Tick()
    {
        if (!ShouldGoToNextWaypoint()) return;

        var currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint == null) return;

        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) return;

        if (currentWaypoint.ActionTime <= 0f) {
            if (interaction.Waypoints.Length <= 1) {
                movement.TryMoveTo(currentWaypoint.Transform);
                return;
            }

            MoveToNextAndAdvance();
        }
        else {
            CurrentWaypointTime += Time.deltaTime;
            if (CurrentWaypointTime >= currentWaypoint.ActionTime) {
                MoveToNextAndAdvance();
            }
        }
    }

    private void MoveToNextAndAdvance()
    {
        UpdateWaypointIndex();
        GoToNextWaypoint();
    }

    public InteractionWaypoint GetCurrentWaypoint()
    {
        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) {
            //Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Interaction is not valid at {this}!");
            return null;
        }

        if (interaction.Waypoints == null || interaction.Waypoints.Length == 0) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] No waypoints in interaction at building {cityNavigator.EnteredBuilding}!");
            return null;
        }

        if (CurrentWaypointIndex < 0 || CurrentWaypointIndex >= interaction.Waypoints.Length) {
            CurrentWaypointIndex = 0;
        }

        var waypoint = interaction.GetWaypoint(CurrentWaypointIndex);
        if (waypoint == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Waypoint is not valid at {cityNavigator.EnteredBuilding}!");
            return null;
        }

        return waypoint;
    }

    private void UpdateWaypointIndex()
    {
        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) return;

        var waypointsLength = interaction.Waypoints.Length;

        if (waypointsLength == 0) {
            Debug.LogWarning($"[{nameof(CreatureWaypointsComponent)}] No waypoints in interaction at building {cityNavigator.EnteredBuilding}!");
            return;
        }

        CurrentWaypointIndex = (CurrentWaypointIndex + 1) % waypointsLength;
        CurrentWaypointTime = 0f;
    }

    private void GoToNextWaypoint()
    {
        var waypoint = GetCurrentWaypoint();
        if (waypoint == null) return;

        movement.TryMoveTo(waypoint.Transform);
    }

    private void ResetWaypoint()
    {
        CurrentWaypointIndex = 0;
        CurrentWaypointTime = 0f;
    }

    private bool ShouldGoToNextWaypoint()
    {
        if (!human.ShouldFollowPath()) return false;

        var targetBuilding = cityNavigator.TargetBuilding;
        if (targetBuilding == null) return false;

        var currentBuilding = cityNavigator.EnteredBuilding;
        if (currentBuilding == null) return false;

        if (currentBuilding != targetBuilding) return false;

        var waypoint = GetCurrentWaypoint();
        if (waypoint == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Current Waypoint is not valid at building {human.CityNavigator.EnteredBuilding}!");
            return false;
        }

        return true;
    }

    private BuildingAction GetCurrentInteractionPoint()
    {
        //if (!cityNavigator.HasPath) return null;

        var currentBuilding = cityNavigator.EnteredBuilding;
        if (currentBuilding == null) {
            //Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Current Building is not valid at {this}!");
            return null;
        }

        return currentBuilding.GetInteractPoint(interactComponent);
    }

    private void HandleInteractionStarted(Building building)
    {
        ResetWaypoint();
    }

    private void HandleInteractionStopped(Building building)
    {
        ResetWaypoint();
    }
}