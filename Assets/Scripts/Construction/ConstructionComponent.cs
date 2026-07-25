using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    [SerializeField] private bool isConstructable = true;
    public bool IsConstructable => isConstructable;

    public long? ConstructionStartTime { get; private set; } = null;
    public long? ConstructionFinishTime { get; private set; } = null;

    public float ConstructionTimeReduction { get; private set; } = 0f;

    public event Action OnConstructionStarted;
    public event Action OnConstructionFinished;

    public static event Action<ConstructionComponent> OnGlobalConstructionStarted;
    public static event Action<ConstructionComponent> OnGlobalConstructionFinished;

    private void OnEnable()
    {
        ConstructionManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        ConstructionManager.Instance.Unregister(this);
    }

    public void Tick()
    {
        if (ConstructionFinishTime == null) return;

        TryFinishConstruction();
    }

    public void Init()
    {
        Init(ConstructionData.Default() ?? new ConstructionData());
    }

    public void Init(ConstructionData constructionData)
    {
        if (constructionData == null) {
            Debug.LogError($"[{nameof(ConstructionComponent)}] Construction Data is not valid");
            Init();
            return;
        }

        ConstructionStartTime = constructionData.ConstructionStartTime;
        ConstructionFinishTime = constructionData.ConstructionFinishTime;
        SetConstructionSpeedBonus(constructionData.ConstructionTimeReduction);

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var constructionTime = ConstructionFinishTime - currentTime;

        if (ShouldFinishConstruction()) {
            FinishConstruction();
        }
        else if (GetUnderConstruction()) {
            OnConstructionStarted?.Invoke();
            OnGlobalConstructionStarted?.Invoke(this);
        }
    }

    public void StartConstruction(int constructionTime)
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ConstructionStartTime = currentTime;
        ConstructionFinishTime = currentTime + constructionTime;

        OnConstructionStarted?.Invoke();
        OnGlobalConstructionStarted?.Invoke(this);
    }

    public void TryFinishConstruction()
    {
        if (!ShouldFinishConstruction()) return;

        FinishConstruction();
    }

    public void FinishConstruction()
    {
        if (ConstructionFinishTime == null) return;

        ConstructionStartTime = null;
        ConstructionFinishTime = null;

        OnConstructionFinished?.Invoke();
        OnGlobalConstructionFinished?.Invoke(this);
    }

    public void SetConstructionSpeedBonus(float value)
    {
        ConstructionTimeReduction = Mathf.Clamp01(value);
    }

    public void ApplyConstructionSpeedBonus()
    {
        var remainingTime = GetRemainingConstructionTime();
        if (remainingTime == null) return;

        ConstructionTimeReduction = Mathf.Clamp01(ConstructionTimeReduction);

        int newRemainingTime = (int)(remainingTime * (1f - ConstructionTimeReduction));
        newRemainingTime = Mathf.Max(0, newRemainingTime);

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ConstructionFinishTime = currentTime + newRemainingTime;
    }

    public int? GetRemainingConstructionTime()
    {
        if (ConstructionFinishTime == null) return null;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timeLeft = (int)(ConstructionFinishTime.Value - currentTime);

        return timeLeft < 0 ? 0 : timeLeft;
    }

    public bool GetUnderConstruction()
    {
        return ConstructionFinishTime != null;
    }

    private bool ShouldFinishConstruction()
    {
        if (ConstructionFinishTime == null) return false;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < ConstructionFinishTime) return false;

        return true;
    }
}