using UnityEngine;

public interface IConnectedBuildingsListener
{
    public void HandleConnectedBuildingInited(Building building);
    public void HandleConnectedBuildingDemolished(Building building);
}
