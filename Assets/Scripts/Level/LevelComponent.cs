using System;
using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    public int Level { get; private set; } = 1;

    public event Action OnLevelChanged;

    public void Init(LevelData levelData)
    {
        SetLevel(levelData.Level);
    }

    public void SetLevel(int value)
    {
        Level = value;
        OnLevelChanged?.Invoke();
    }
}