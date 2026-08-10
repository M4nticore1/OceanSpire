using System;
using UnityEngine;

public class CreatureWaypointsComponent : MonoBehaviour
{
    [SerializeField] private Human human;

    public int CurrentWaypointIndex { get; private set; } = 0;
    public float CurrentWaypointTime { get; private set; } = 0f;

    private void OnEnable()
    {
        human.InteractComponent.OnInteractionStarted += HandleInteractionStarted;
        human.InteractComponent.OnInteractionStopped += HandleInteractionStopped;

        CreatureWaypointsManager.Instance.RegisterComponent(this);
    }

    private void OnDisable()
    {
        human.InteractComponent.OnInteractionStarted -= HandleInteractionStarted;
        human.InteractComponent.OnInteractionStopped -= HandleInteractionStopped;

        CreatureWaypointsManager.Instance.UnregisterComponent(this);
    }

    public void Init()
    {
        Init(CreatureWaypointsComponentData.Default());
    }

    public void Init(CreatureWaypointsComponentData waypointsData)
    {
        if (waypointsData == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Waypoints Data is not valid!");
            Init();
            return;
        }

        CurrentWaypointIndex = waypointsData.CurrentWaypointIndex;
        CurrentWaypointTime = waypointsData.CurrentWaypointTime;
    }

    public void Tick()
    {
        if (!human.ShouldFollowPath()) return;

        var cityNavigator = human.CityNavigator;

        var targetBuilding = cityNavigator.TargetBuilding;
        if (targetBuilding == null) return;

        var currentBuilding = cityNavigator.CurrentBuilding;
        if (currentBuilding == null) return;

        if (currentBuilding != targetBuilding) return;

        CurrentWaypointTime += Time.deltaTime;
        if (!ShouldGoToNextWaypoint()) return;

        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) return;
        if (interaction.GetWaypoint(CurrentWaypointIndex).ActionTime <= 0) return;

        UpdateWaypoint();
        GoToNextWaypoint();
    }

    public InteractionWaypoint GetCurrentWaypoint()
    {
        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) return null;

        return interaction.GetWaypoint(CurrentWaypointIndex);
    }

    private void UpdateWaypoint()
    {
        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) return;

        var waypointsLength = interaction.Waypoints.Length;
        if (waypointsLength == 0) {
            Debug.LogWarning($"[{nameof(CreatureWaypointsComponent)}] No waypoints in interaction at building {human.CityNavigator.CurrentBuilding}!");
            return;
        }

        CurrentWaypointIndex = (CurrentWaypointIndex + 1) % waypointsLength;
        CurrentWaypointTime = 0;
    }

    private void GoToNextWaypoint()
    {
        var waypoint = GetCurrentWaypoint();
        if (waypoint == null) return;

        human.Movement.TryMoveTo(waypoint.Transform);
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
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Current Waypoint is not valid at building {human.CityNavigator.CurrentBuilding}!");
            return false;
        }

        if (CurrentWaypointTime < waypoint.ActionTime) return false;

        return true;
    }

    private BuildingAction GetCurrentInteractionPoint()
    {
        var cityNavigator = human.CityNavigator;
        if (!cityNavigator.HasPath) return null;

        var CurrentBuilding = cityNavigator.CurrentBuilding;
        if (CurrentBuilding == null) return null;

        return CurrentBuilding.GetInteractPoint(human);
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