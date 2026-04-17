using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    public int level { get; private set; } = 1;

    public void SetLevel(int value)
    {
        level = value;
    }
}