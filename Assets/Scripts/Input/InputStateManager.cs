using UnityEngine;

public class InputStateManager : MonoBehaviour
{
    public static InputStateManager Instance;
    public bool IsGameplayInputBlocked { get; private set; } = false;
    private int blockTargetsCount = 0;

    private void Awake()
    {
        if (Instance) return;

        Instance = this;
    }

    public void AddBlockTarget()
    {
        SetTargetsCount(blockTargetsCount + 1);
        UpdateInputBlocked();
    }

    public void RemoveBlockTarget()
    {
        SetTargetsCount(blockTargetsCount - 1);
        UpdateInputBlocked();
    }

    private void SetTargetsCount(int value)
    {
        blockTargetsCount = Mathf.Max(0, value);
    }

    private void UpdateInputBlocked()
    {
        IsGameplayInputBlocked = blockTargetsCount > 0;
    }
}