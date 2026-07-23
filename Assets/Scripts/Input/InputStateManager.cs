using System.Collections.Generic;
using UnityEngine;

public class InputStateManager : MonoBehaviour
{
    public static InputStateManager Instance;
    public bool IsGameplayInputBlocked { get; private set; } = false;
    private List<MonoBehaviour> blockTargets = new();

    private void Awake()
    {
        if (Instance) return;

        Instance = this;
    }

    public void AddBlockTarget(MonoBehaviour blockTarget)
    {
        if (!blockTarget) return;
        if (blockTargets.Contains(blockTarget)) {
            Debug.LogError($"[{nameof(InputStateManager)}] Manager already contains {blockTarget}!");
            return;
        }

        blockTargets.Add(blockTarget);
        UpdateInputBlocked();
    }

    public void RemoveBlockTarget(MonoBehaviour blockTarget)
    {
        if (!blockTarget) return;

        blockTargets.Remove(blockTarget);
        UpdateInputBlocked();
    }

    private void UpdateInputBlocked()
    {
        IsGameplayInputBlocked = blockTargets.Count > 0;
    }
}