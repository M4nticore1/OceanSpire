using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    private int levelIndex = 0;
    public int LevelIndex => levelIndex;

    public void SetLevelIndex(int value)
    {
        levelIndex = value;
    }
}
