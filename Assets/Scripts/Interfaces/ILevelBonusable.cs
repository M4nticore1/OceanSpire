using UnityEngine;

public interface ILevelBonusable
{
    public float LevelBonus { get; } // Default 0f
    public void SetLevelBonus(float bonus);
}
