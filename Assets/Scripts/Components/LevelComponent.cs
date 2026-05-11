using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    public int Level { get; private set; } = 1;

    public void SetLevel(int value)
    {
        Level = value;
    }
}