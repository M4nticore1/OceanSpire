using System;

public static class FPSCounterSystem
{
    public static event Action<bool> onCountingEnabledChanged;

    public static void SetCounterEnabled(bool value)
    {
        onCountingEnabledChanged?.Invoke(value);
    }
}