using UnityEngine;

public class HealthData
{
    public float CurrentHealth = 0;

    public static HealthData Default()
    {
        return new HealthData();
    }

    public static HealthData Create(HealthComponent healthComponent)
    {
        return new HealthData()
        {
            CurrentHealth = healthComponent.CurrentHealth,
        };
    }
}