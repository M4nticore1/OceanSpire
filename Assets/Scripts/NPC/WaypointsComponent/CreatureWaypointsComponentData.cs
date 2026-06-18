using UnityEngine;

public class CreatureWaypointsComponentData
{
    public int CurrentWaypointIndex = 0;
    public float CurrentWaypointTime = 0f;

    public static CreatureWaypointsComponentData Default()
    {
        return new CreatureWaypointsComponentData();
    }

    public static CreatureWaypointsComponentData Create(CreatureWaypointsComponent waypointsComponent)
    {
        return new CreatureWaypointsComponentData()
        {
            CurrentWaypointIndex = waypointsComponent.CurrentWaypointIndex,
            CurrentWaypointTime = waypointsComponent.CurrentWaypointTime,
        };
    }
}