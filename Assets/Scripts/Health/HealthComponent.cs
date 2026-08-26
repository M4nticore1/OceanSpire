using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour, ILocalizable
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    [SerializeField] private float currentHealth = 0;
    public float CurrentHealth => currentHealth;

    [field: SerializeField] public bool IsAlive { get; private set; } = true;

    public event Action OnHealthChanged;
    public event Action OnDied;

    public void Init()
    {
        Init(HealthData.Default() ?? new HealthData());
    }

    public void Init(HealthData healthData)
    {
        if (healthData == null) {
            Debug.LogError("healthData is not valid", this);
            Init();
            return;
        }

        SetCurrentHealth(healthData.CurrentHealth);
    }

    public void SetMaxHealh(float value)
    {
        maxHealth = value;
    }

    public void AddHealth(float value)
    {
        if (CurrentHealth >= maxHealth) return;

        SetCurrentHealth(CurrentHealth + value);
    }

    public void RemoveHealth(float value)
    {
        if (CurrentHealth < 0f) return;

        SetCurrentHealth(CurrentHealth - value);
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        OnHealthChanged?.Invoke();

        if (ShouldDie()) {
            Die();
        }
        else if (ShouldRevive()) {
            Revive();
        }
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "currentHealth", Mathf.Max(Mathf.CeilToInt(CurrentHealth), maxHealth).ToString() },
            { "maxHealth", Mathf.Max(Mathf.CeilToInt(MaxHealth), maxHealth).ToString() },
            { "currentHealthPercent", MaxHealth > 0 ? Mathf.CeilToInt(CurrentHealth / MaxHealth * 100).ToString(): "0" },
        };
    }

    private void Revive()
    {
        IsAlive = true;
    }

    private void Die()
    {
        IsAlive = false;
        OnDied?.Invoke();
    }

    private bool ShouldDie()
    {
        if (CurrentHealth > 0) return false;
        if (!IsAlive) return false;

        return true;
    }

    private bool ShouldRevive()
    {
        if (CurrentHealth <= 0) return false;
        if (IsAlive) return false;

        return true;
    }
}