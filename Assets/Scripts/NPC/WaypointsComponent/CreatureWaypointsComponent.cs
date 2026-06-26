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

    public void Tick()
    {
        if (!interactComponent.InteractBuilding) return;
        if (!interactComponent.IsInteracting) return;

        CurrentWaypointTime += Time.deltaTime;

        if (!ShouldGoToNextWaypoint()) return;

        UpdateWaypoint();
        GoToNextWaypoint();
    }

    public void Init(CreatureWaypointsComponentData waypointsData)
    {
        if (waypointsData == null) {
            Debug.LogError("waypointsData is not valid", this);
            return;
        }

        CurrentWaypointIndex = waypointsData.CurrentWaypointIndex;
        CurrentWaypointTime = waypointsData.CurrentWaypointTime;
    }

    public BuildingActionWaypoint GetCurrentWaypoint()
    {
        var interaction = GetCurrentBuildingInteraction();
        if (interaction == null) {
            Debug.LogError("currentBuildingInteraction is not valid");
            return null;
        }

        return interaction.GetWaypoint(CurrentWaypointIndex);
    }

    private void UpdateWaypoint()
    {
        var interaction = GetCurrentBuildingInteraction();
        if (interaction == null) {
            Debug.LogError("currentBuildingInteraction is not valid");
            return;
        }

        var waypointsLength = interaction.Waypoints.Length;
        if (waypointsLength == 0) {
            Debug.LogWarning("No waypoints in interaction");
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
            Debug.LogError("CurrentWaypoint is not valid");
            return false;
        }

        if (CurrentWaypointTime < waypoint.ActionTime) return false;

        return true;
    }

    private BuildingAction GetCurrentBuildingInteraction()
    {
        var interactBuilding = interactComponent.InteractBuilding;
        if (!interactBuilding) {
            Debug.LogError("interactBuilding is not valid ", this);
            return null;
        }

        var construction = interactBuilding.SpawnedConstruction;
        if (!construction) {
            Debug.LogError("construction is not valid ", this);
            return null;
        }

        return construction.GetInteraction(cityNavigator);
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