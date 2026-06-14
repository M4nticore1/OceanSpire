using System;
using UnityEngine;

public class ReviveComponent : MonoBehaviour
{
    [SerializeField] private HealthComponent health;
    [SerializeField] private float reviveHealthPercent = 0.1f;

    [SerializeField] private float reviveLimitTime = 600f;
    public float ReviveLimitTime => reviveLimitTime;

    public float CurrentDiedTime { get; private set; }

    public event Action OnRevived;
    public event Action onLimitTimeOvered;

    private void OnEnable()
    {
        health.OnDied += OnDied;
    }

    private void OnDisable()
    {
        health.OnDied -= OnDied;
    }

    private void Update()
    {
        if (health.IsAlive) return;

        CurrentDiedTime += Time.deltaTime;

        if (CurrentDiedTime >= reviveLimitTime) {
            OnLimitTimeUp();
        }
    }

    public void Revive()
    {
        health.SetCurrentHealth(GetReviveHealth());
        OnRevived?.Invoke();
    }

    private void ResetDiedTime()
    {
        CurrentDiedTime = 0f;
    }

    private void OnDied()
    {
        ResetDiedTime();
    }

    private void OnLimitTimeUp()
    {
        onLimitTimeOvered?.Invoke();
    }

    private float GetReviveHealth()
    {
        return health.MaxHealth * reviveHealthPercent;
    }
}