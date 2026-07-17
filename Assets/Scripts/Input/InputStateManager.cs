using UnityEngine;

public class InputStateManager : MonoBehaviour
{
    public static InputStateManager Instance;

    public bool isGameplayInputBlocked { get; private set; } = false;

    public void SetGameplayInputBlocked(bool value)
    {
        isGameplayInputBlocked = value;
    }

    private void Awake()
    {
        Instance = this;
    }
}