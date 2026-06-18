using UnityEngine;

public class CityNavigatorData
{
    public int? EnteredBuildingInstanceId = null;

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
            ElevatorPassenger = ElevatorPassengerData.Create(cityNavigator.ElevatorPassenger),
            Waypoints = CreatureWaypointsComponentData.Create(cityNavigator.WaypointsComponent)
        };
    }
}