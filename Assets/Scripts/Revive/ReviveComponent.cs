using System;
using System.Collections.Generic;
using UnityEngine;

public class ReviveComponent : MonoBehaviour, ILocalizable
{
    [SerializeField] private HealthComponent health;
    [SerializeField] private float reviveHealthPercent = 0.1f;

    [SerializeField] private int reviveLimitTime = 600;
    public int ReviveLimitTime => reviveLimitTime;

    public long? DieTime { get; private set; } = null;

    public event Action OnRevived;
    public event Action onLimitTimeOvered;

    public static event Action<ReviveComponent> OnGlobalRevived;

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
        if (DieTime == null) return;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < DieTime) return;

        OnLimitTimeUp();
    }

    public void Init()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var reviveData = new ReviveData()
        {
            DieTime = health.IsAlive ? null : currentTime + reviveLimitTime
        };

        Init(reviveData);
    }

    public void Init(ReviveData reviveData)
    {
        if (reviveData == null) {
            Debug.LogError("reviveData is not valid");
            return;
        }

        DieTime = reviveData.DieTime;
    }

    public void Revive()
    {
        health.SetCurrentHealth(GetReviveHealth());

        OnRevived?.Invoke();
        OnGlobalRevived?.Invoke(this);
    }

    public Dictionary<string, string> GetLocalization()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return new Dictionary<string, string>()
        {
            { "remainingTime", DieTime != null ? TimeFormatter.SecondsToMinuteTime((int)(DieTime - currentTime)) : TimeFormatter.SecondsToMinuteTime(0) }
        };
    }

    private void ResetDiedTime()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DieTime = currentTime + reviveLimitTime;
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