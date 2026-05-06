using UnityEngine;

public class IntRef
{
    public int Value { get; set; } = 0;

    public static IntRef operator +(IntRef a, int b)
    {
        a.Value += b;
        return a;
    }

    public static IntRef operator -(IntRef a, int b)
    {
        a.Value -= b;
        return a;
    }
}
