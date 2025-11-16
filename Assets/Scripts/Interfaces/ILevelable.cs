using UnityEngine;

public interface ILevelable
{
    int LevelIndex { get; }
    void SetLevel(int levelIndex);
}
