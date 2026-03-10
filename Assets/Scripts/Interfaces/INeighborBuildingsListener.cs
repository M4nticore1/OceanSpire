using UnityEngine;

public interface INeighborBuildingsListener
{
    public void HandleNeighborBuildingInited(TowerBuilding building);
    public void HandleNeighborBuildingDemolished(TowerBuilding building);
}
