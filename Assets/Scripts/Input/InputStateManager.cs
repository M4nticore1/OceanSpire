using UnityEngine;

public class InputStateManager : MonoBehaviour
{
    public bool isGameplayInputBlocked { get; private set; } = false;

    public void SetGameplayInputBlocked(bool value)
    {
        isGameplayInputBlocked = value;
    }
}
