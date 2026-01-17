using UnityEngine;

public interface ILevelable
{
    int LevelIndex { get; set; }
    void SetLevel(int levelIndex);
}
