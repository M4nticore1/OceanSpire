using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    public float currentHealth { get; private set; } = 0;
    public bool isAlive { get; private set; } = true;

    public event Action onHealthChanged;
    public event Action onRevived;
    public event Action onDied;

    public void Init(float currentHealth)
    {
        SetCurrentHealth(currentHealth);
    }

    public void SetMaxHealh(float value)
    {
        maxHealth = value;
    }

    public void AddHealth(float value)
    {
        SetCurrentHealth(currentHealth + value);
    }

    public void RemoveHealth(float value)
    {
        SetCurrentHealth(currentHealth - value);
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = value;
        onHealthChanged?.Invoke();

        if (currentHealth <= 0 && isAlive) {
            OnDied();
        }
        else if (currentHealth > 0 && !isAlive) {
            OnRevived();
        }
    }

    public void OnRevived()
    {
        isAlive = true;
        onRevived?.Invoke();
    }

    private void OnDied()
    {
        isAlive = false;
        onDied?.Invoke();
    }
}