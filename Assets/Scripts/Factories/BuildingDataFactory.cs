using UnityEngine;

public static class BuildingDataFactory
{
    public static BuildingData CreateBuildingData(Building building)
    {
        int id = building.BuildingData.BuildingId;
        int instanceId = InstancesManager.instance.GetNextInstanceId();
        ConstructionData constructionData = new ConstructionData(0, building.ConstructionComponent.IsConstructable);

        BuildingData data = new BuildingData(id, instanceId, 1, constructionData);
        return data;
    }

    public static TowerBuildingData CreateBuildingData(TowerBuilding building, int floorIndex, int placeIndex)
    {
        BuildingData data = CreateBuildingData(building);

        TowerBuildingData towerBuildingData = new TowerBuildingData(data.Id, data.InstanceId, data.Level, data.ConstructionData, floorIndex, placeIndex);
        return towerBuildingData;
    }
}
