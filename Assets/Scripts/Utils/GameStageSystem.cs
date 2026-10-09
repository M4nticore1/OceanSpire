using UnityEngine;

public static class GameStageSystem
{
    private static BuildingsManager buildingsManager => BuildingsManager.Instance;

    public static float CalculateGameStagePercent()
    {
        if (buildingsManager == null) return 0;

        return (float)buildingsManager.RoomBuildings.Count / buildingsManager.GetMaxRoomBuildingsCount();
    }
}