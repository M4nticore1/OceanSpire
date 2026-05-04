using UnityEngine;

public static class GameStageSystem
{
    public static float CalculateGameStagePercent()
    {
        return (float)BuildingsManager.instance.BuiltFloors.Count / BuildingsManager.instance.MaxFloorsCount;
    }
}
