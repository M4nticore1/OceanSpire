using UnityEngine;

public interface IElevatorPassenger
{
    public int FloorIndex { get; }
    public void OnElevatorChangedFloor(Building building);
    public void OnElevatorStopped();
}
