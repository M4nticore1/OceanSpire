using UnityEngine;

public class InputStateManager : MonoBehaviour
{
    public bool isGameplayInputBlocked { get; private set; } = false;

    public void SetGamePlayInputBlocked(bool value)
    {
        isGameplayInputBlocked = value;
    }
}