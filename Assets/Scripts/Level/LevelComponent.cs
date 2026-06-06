using System;
using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    public int Level { get; private set; } = 1;

    public event Action OnLevelChanged;

    public void Init(LevelData levelData)
    {
        TrySetLevel(levelData.Level);
    }

    public void TrySetLevel(int value)
    {
        if (!ShouldSetLevel(value)) return;

        SetLevel(value);
    }

    private void SetLevel(int value)
    {
        Level = value;
        OnLevelChanged?.Invoke();
    }

    private bool ShouldSetLevel(int value)
    {
        if (Level == value) return false;

        return true;
    }
}