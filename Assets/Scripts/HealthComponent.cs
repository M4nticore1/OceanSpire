using System;
using Unity.Mathematics;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth { get { return maxHealth; } }
    private float currentHealth = 0;
    public float CurrentHealth { get { return currentHealth; } }

    public event Action onHealthChanged;

    public void AddHealth(float value)
    {
        SetHealth(currentHealth + value);
    }

    public void RemoveHealth(float value)
    {
        SetHealth(currentHealth - value);
    }

    public void SetHealth(float value)
    {
        float newHealth = math.clamp(value, 0, MaxHealth - CurrentHealth);
        currentHealth = value;
        if (CurrentHealth <= 0) {
            Die();
        }
        onHealthChanged?.Invoke();
    }

    private void Die()
    {

    }
}
