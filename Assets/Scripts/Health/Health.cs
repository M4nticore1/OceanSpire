using System;
using Unity.Mathematics;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    public float currentHealth { get; private set; } = 0;
    public bool isAlive { get; private set; } = true;

    public event Action onHealthChanged;
    public event Action onRevived;
    public event Action onDeath;

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

        if (currentHealth <= 0 && isAlive) {
            Death();
        }
        else if (!isAlive) {
            Revive();
        }

        onHealthChanged?.Invoke();
    }

    private void Revive()
    {
        isAlive = true;
        onRevived?.Invoke();
    }

    private void Death()
    {
        isAlive = false;
        onDeath?.Invoke();
    }
}