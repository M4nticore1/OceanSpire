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
        if (!ShouldGoToNextWaypoint()) return;

        var currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint == null) return;

        var interaction = GetCurrentInteractionPoint();
        if (currentWaypoint.ActionTime <= 0f) {
            if (interaction.Waypoints.Length <= 1) {
                human.Movement.TryMoveTo(currentWaypoint.Transform);
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
        if (interaction == null) return null;

        if (CurrentWaypointIndex < 0 || CurrentWaypointIndex >= interaction.Waypoints.Length) {
            CurrentWaypointIndex = 0;
        }

        return interaction.GetWaypoint(CurrentWaypointIndex);
    }

    private void UpdateWaypointIndex()
    {
        var interaction = GetCurrentInteractionPoint();
        if (interaction == null) return;

        var waypointsLength = interaction.Waypoints.Length;
        if (waypointsLength == 0) {
            Debug.LogWarning($"[{nameof(CreatureWaypointsComponent)}] No waypoints in interaction at building {human.CityNavigator.CurrentBuilding}!");
            return;
        }

        CurrentWaypointIndex = (CurrentWaypointIndex + 1) % waypointsLength;
        CurrentWaypointTime = 0f;
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
        if (!human.ShouldFollowPath()) return false;

        var cityNavigator = human.CityNavigator;

        var targetBuilding = cityNavigator.TargetBuilding;
        if (targetBuilding == null) return false;

        var currentBuilding = cityNavigator.CurrentBuilding;
        if (currentBuilding == null) return false;

        if (currentBuilding != targetBuilding) return false;

        var waypoint = GetCurrentWaypoint();
        if (waypoint == null) {
            Debug.LogError($"[{nameof(CreatureWaypointsComponent)}] Current Waypoint is not valid at building {human.CityNavigator.CurrentBuilding}!");
            return false;
        }

        return true;
    }

    private BuildingAction GetCurrentInteractionPoint()
    {
        var cityNavigator = human.CityNavigator;
        if (!cityNavigator.HasPath) return null;

        var currentBuilding = cityNavigator.CurrentBuilding;
        if (currentBuilding == null) return null;

        return currentBuilding.GetInteractPoint(human);
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