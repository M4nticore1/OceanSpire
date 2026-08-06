using System;
using UnityEngine;

public class CreatureWaypointsComponent : MonoBehaviour
{
    [SerializeField] private Movement movement;
    [SerializeField] private CreatureInteractComponent interactComponent;
    [SerializeField] private CreatureCityNavigator cityNavigator;

    public int CurrentWaypointIndex { get; private set; } = 0;
    public float CurrentWaypointTime { get; private set; } = 0f;

    private void OnEnable()
    {
        interactComponent.OnInteractionStarted += OnInteractionStarted;
        interactComponent.OnInteractionStopped += OnInteractionStopped;

        CreatureWaypointsManager.Instance.RegisterComponent(this);
    }

    private void OnDisable()
    {
        interactComponent.OnInteractionStarted -= OnInteractionStarted;
        interactComponent.OnInteractionStopped -= OnInteractionStopped;

        CreatureWaypointsManager.Instance.UnregisterComponent(this);
    }

    public void Init(CreatureWaypointsComponentData waypointsData)
    {
        if (waypointsData == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Waypoints Data is not valid!");
            return;
        }

        CurrentWaypointIndex = waypointsData.CurrentWaypointIndex;
        CurrentWaypointTime = waypointsData.CurrentWaypointTime;
    }

    public void Tick()
    {
        if (!interactComponent.InteractBuilding) return;
        if (!interactComponent.IsInteracting) return;

        var interaction = GetCurrentInteractionPoint();
        if (interaction.GetWaypoint(CurrentWaypointIndex).ActionTime <= 0) return;

        CurrentWaypointTime += Time.deltaTime;
        if (!ShouldGoToNextWaypoint()) return;

        UpdateWaypoint();
        GoToNextWaypoint();
    }

    public BuildingActionWaypoint GetCurrentWaypoint()
    {
        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Current Building Interaction is not valid at building {cityNavigator.CurrentBuilding}!");
            return null;
        }

        return interaction.GetWaypoint(CurrentWaypointIndex);
    }

    private void UpdateWaypoint()
    {
        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Current Building Interaction is not valid at building {cityNavigator.CurrentBuilding}!");
            return;
        }

        var waypointsLength = interaction.Waypoints.Length;
        if (waypointsLength == 0) {
            Debug.LogWarning($"[{nameof(CreatureWaypointsComponent)}] No waypoints in interaction at building {cityNavigator.CurrentBuilding}!");
            return;
        }

        CurrentWaypointIndex = (CurrentWaypointIndex + 1) % waypointsLength;
        CurrentWaypointTime = 0;
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
        var waypoint = GetCurrentWaypoint();
        if (waypoint == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Current Waypoint is not valid at building {cityNavigator.CurrentBuilding}!");
            return false;
        }

        if (CurrentWaypointTime < waypoint.ActionTime) return false;

        return true;
    }

    private BuildingAction GetCurrentInteractionPoint()
    {
        var targetBuilding = cityNavigator.TargetBuilding;
        if (!targetBuilding) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Target Building is not valid!");
            return null;
        }

        var human = GetComponent<Human>();
        if (!human) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Human is not valid!");
            return null;
        }

        return targetBuilding.GetInteractPoint(human);
    }

    private void OnInteractionStarted(Building building)
    {
        ResetWaypoint();
    }

    private void OnInteractionStopped(Building building)
    {
        ResetWaypoint();
    }
}