using UnityEngine;

public interface IElevatorPassenger
{
    public int floorIndex { get; }
    public void OnElevatorChangedFloor(Building building);
    public void OnElevatorStopped();
}
