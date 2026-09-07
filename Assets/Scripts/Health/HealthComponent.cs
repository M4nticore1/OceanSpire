using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour, ILevelBonusable, ILocalizable
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    [SerializeField] private float currentHealth = 0;
    public float CurrentHealth => currentHealth;

    [field: SerializeField] public bool IsAlive { get; private set; } = true;
    [field: SerializeField] public float LevelBonus { get; private set; } = 0f;

    public event Action OnHealthChanged;
    public event Action OnDied;

    public Action OnInited;
    public Action OnDestroyed;

    public static Action<float> OnHealed;
    public static Action<float> OnDamaged;

    public static Action<HealthComponent, float> OnGlobalHealed;
    public static Action<HealthComponent, float> OnGlobalDamaged;

    public static Action<HealthComponent> OnGlobalInited;
    public static Action<HealthComponent> OnGlobalDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
        OnGlobalDestroyed?.Invoke(this);
    }

    public void Init()
    {
        Init(HealthData.Default() ?? new HealthData());
    }

    public void Init(HealthData healthData)
    {
        if (healthData == null) {
            Debug.LogError($"[{nameof(HealthComponent)}] Health Data is not valid!");
            Init();
            return;
        }

        SetCurrentHealth(healthData.CurrentHealth);

        OnInited?.Invoke();
        OnGlobalInited?.Invoke(this);
    }

    public void SetMaxHealh(float value)
    {
        maxHealth = value;
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

    public void AddHealth(float value, bool useHealBonus)
    {
        if (CurrentHealth >= maxHealth) return;

        if (useHealBonus) {
            value *= 1f + LevelBonus;
        }

        SetCurrentHealth(CurrentHealth + value);

        OnHealed?.Invoke(value);
        OnGlobalHealed?.Invoke(this, value);
    }

    public void RemoveHealth(float value)
    {
        if (CurrentHealth < 0f) return;

        SetCurrentHealth(CurrentHealth - value);

        OnDamaged?.Invoke(value);
        OnGlobalDamaged?.Invoke(this, value);
    }

    // Bonus
    public void SetLevelBonus(float bonus)
    {
        LevelBonus = bonus;
    }

    // Localization
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