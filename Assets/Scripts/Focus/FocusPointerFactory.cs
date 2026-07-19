using UnityEngine;

public static class FocusPointerFactory
{
    public static FocusPointer CreatePointer(FocusPointer prefab, Transform transform)
    {
        var pointer = GameObject.Instantiate(prefab, transform);

        return pointer;
    }
}