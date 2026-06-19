using UnityEngine;

public class CityNavigatorData
{
    public int? EnteredBuildingInstanceId = null;
    public int? TargetBuildingInstanceId = null;

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
            EnteredBuildingInstanceId = cityNavigator.CurrentBuilding?.InstanceId.GetId(),
            TargetBuildingInstanceId = cityNavigator.TargetBuilding?.InstanceId.GetId(),
            ElevatorPassenger = ElevatorPassengerData.Create(cityNavigator.ElevatorPassenger),
            Waypoints = CreatureWaypointsComponentData.Create(cityNavigator.WaypointsComponent)
        };
    }
}