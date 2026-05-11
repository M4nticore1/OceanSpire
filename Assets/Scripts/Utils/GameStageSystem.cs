using UnityEngine;

public static class GameStageSystem
{
    public static float CalculateGameStagePercent()
    {
        return (float)BuildingsManager.Instance.BuiltFloors.Count / BuildingsManager.Instance.MaxFloorsCount;
    }
}
