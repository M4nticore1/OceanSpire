using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    public float currentHealth { get; private set; } = 0;
    public bool IsAlive { get; private set; } = true;

    public event Action onHealthChanged;
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
        if (currentHealth >= maxHealth) return;

        SetCurrentHealth(currentHealth + value);
    }

    public void RemoveHealth(float value)
    {
        if (currentHealth < 0f) return;

        SetCurrentHealth(currentHealth - value);
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = value;
        onHealthChanged?.Invoke();

        if (ShouldDie()) {
            OnDied();
        }
        else if (ShouldRevive()) {
            OnRevived();
        }
    }

    private void OnRevived()
    {
        IsAlive = true;
    }

    private void OnDied()
    {
        IsAlive = false;
        onDied?.Invoke();
    }

    private bool ShouldDie()
    {
        if (currentHealth > 0) return false;
        if (!IsAlive) return false;

        return true;
    }

    private bool ShouldRevive()
    {
        if (currentHealth <= 0) return false;
        if (IsAlive) return false;

        return true;
    }
}