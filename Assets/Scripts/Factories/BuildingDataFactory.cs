//using UnityEngine;

//public static class BuildingDataFactory
//{
//    public static BuildingData CreateBuildingData(Building building)
//    {
//        int id = building.BuildingData.BuildingId;
//        int instanceId = InstancesManager.instance.GetNextInstanceId();

//        ConstructionData constructionData = new ConstructionData()
//        {
//            ConstructionTime = 0f,
//            UnderConstruction = building.ConstructionComponent.IsConstructable
//        };

//        BuildingData data = new BuildingData()
//        {
//            Id = id,
//            InstanceId = instanceId,
//            Level =
//        };

//        return data;
//    }

//    public static TowerBuildingData CreateBuildingData(TowerBuilding building, int floorIndex, int placeIndex)
//    {
//        BuildingData data = CreateBuildingData(building);

//        TowerBuildingData towerBuildingData = new TowerBuildingData(data.Id, data.InstanceId, data.Level, data.Construction, floorIndex, placeIndex);
//        return towerBuildingData;
//    }
//}