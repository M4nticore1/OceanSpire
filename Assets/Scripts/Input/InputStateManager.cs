using UnityEngine;

public class InputStateManager : MonoBehaviour
{
    public static InputStateManager instance;

    public bool isGameplayInputBlocked { get; private set; } = false;

    public void SetGameplayInputBlocked(bool value)
    {
        isGameplayInputBlocked = value;
    }

    private void Awake()
    {
        instance = this;
    }
}
