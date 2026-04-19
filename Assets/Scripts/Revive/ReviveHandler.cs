using UnityEngine;

public class ReviveHandler : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private float reviveHealthPercent = 0.1f;

    [SerializeField] private float reviveLimitTime = 600f;
    public float ReviveLimitTime => reviveLimitTime;

    public float CurrentDiedTime { get; private set; }

    private void OnEnable()
    {
        health.onDied += OnDied;
    }

    private void OnDisable()
    {
        health.onDied -= OnDied;
    }

    private void Update()
    {
        if (health.isAlive) return;

        CurrentDiedTime += Time.deltaTime;
    }

    public void Revive()
    {
        Debug.Log("Revive");
        health.SetCurrentHealth(GetReviveHealth());
    }

    private void ResetDiedTime()
    {
        CurrentDiedTime = 0f;
    }

    private void OnDied()
    {
        ResetDiedTime();
    }

    private float GetReviveHealth()
    {
        return health.MaxHealth * reviveHealthPercent;
    }
}