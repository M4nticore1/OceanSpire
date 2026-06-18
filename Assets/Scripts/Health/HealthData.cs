using UnityEngine;

public class HealthData
{
    public float CurrentHealth;

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