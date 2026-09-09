using System.Collections.Generic;
using UnityEngine;

public class InputStateManager : MonoBehaviour
{
    public static InputStateManager Instance;
    public bool IsGameplayInputBlocked { get; private set; } = false;
    private List<MonoBehaviour> inputBlockTargets = new();

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddInputBlockTarget(MonoBehaviour blockTarget)
    {
        if (blockTarget == null) return;
        if (inputBlockTargets.Contains(blockTarget)) {
            Debug.LogError($"[{nameof(InputStateManager)}] Manager already contains {blockTarget}!");
            return;
        }

        inputBlockTargets.Add(blockTarget);
        UpdateInputBlocked();
    }

    public void RemoveBlockTarget(MonoBehaviour blockTarget)
    {
        if (blockTarget == null) return;

        inputBlockTargets.Remove(blockTarget);
        UpdateInputBlocked();
    }

    private void UpdateInputBlocked()
    {
        IsGameplayInputBlocked = inputBlockTargets.Count > 0;
    }
}