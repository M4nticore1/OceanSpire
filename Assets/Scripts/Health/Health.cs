using System;
using Unity.Mathematics;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth { get { return maxHealth; } }
    private float currentHealth = 0;
    public float CurrentHealth { get { return currentHealth; } }

    public event Action onHealthChanged;

    public void Init(float currentHealth)
    {
        SetMaxHealh(maxHealth);
        SetCurrentHealth(currentHealth);
    }

    public void AddHealth(float value)
    {
        SetCurrentHealth(currentHealth + value);
    }

    public void RemoveHealth(float value)
    {
        SetCurrentHealth(currentHealth - value);
    }

    public void SetMaxHealh(float value)
    {
        maxHealth = value;
    }

    public void SetCurrentHealth(float value)
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
