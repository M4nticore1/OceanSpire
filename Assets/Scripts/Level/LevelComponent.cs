using System;
using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    [field: SerializeField] public int Level { get; private set; } = 1;

    public event Action OnLevelChanged;

    public void Init(int level)
    {
        TrySetLevel(level);
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