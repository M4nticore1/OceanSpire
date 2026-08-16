using System;
using UnityEngine;

public class CityNavigatorData
{
    public Guid? EnteredBuildingInstanceId = null;
    public Guid? TargetBuildingInstanceId = null;
    public ElevatorPassengerData ElevatorPassenger = ElevatorPassengerData.Default();
    public CreatureWaypointsComponentData Waypoints = CreatureWaypointsComponentData.Default();

    public static CityNavigatorData Default()
    {
        return new CityNavigatorData();
    }

    public static CityNavigatorData Create(CreatureCityNavigator cityNavigator)
    {
        return new CityNavigatorData()
        {
            EnteredBuildingInstanceId = cityNavigator.EnteredBuilding?.InstanceId.GetGuid(),
            TargetBuildingInstanceId = cityNavigator.TargetBuilding?.InstanceId.GetGuid(),
            ElevatorPassenger = ElevatorPassengerData.Create(cityNavigator.ElevatorPassenger),
            Waypoints = CreatureWaypointsComponentData.Create(cityNavigator.WaypointsComponent)
        };
    }
}