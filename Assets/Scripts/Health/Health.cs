using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    [SerializeField] private float reviveHealthPercent = 0.1f;

    public float currentHealth { get; private set; } = 0;
    public bool isAlive { get; private set; } = true;

    public event Action onHealthChanged;
    public event Action onRevived;
    public event Action onDied;

    public void Init(float currentHealth)
    {
        SetCurrentHealth(currentHealth);
    }

    public void Revive()
    {
        float health = maxHealth * reviveHealthPercent;
        SetCurrentHealth(health);
        OnRevived();
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
        else if (!isAlive) {
            OnRevived();
        }
    }

    private void OnRevived()
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