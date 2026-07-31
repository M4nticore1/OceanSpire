using System;
using System.Collections.Generic;
using UnityEngine;

public class BuilderEnergyManager : MonoBehaviour, ILocalizable
{
    public static BuilderEnergyManager Instance { get; private set; }

    [SerializeField] private BuildingsLoader buildingsLoader;

    [SerializeField] private float chargeEnergyPower = 0.1f;
    public float ChargeEnergyPower => chargeEnergyPower;

    [SerializeField] private int chargeEnergyFrequency = 1800;
    public int ChargeEnergyFrequency => chargeEnergyFrequency;

    [SerializeField] private float energySpend = 0.3f;
    public float EnergySpend => energySpend;

    public float CurrentEnergy { get; private set; } = 1f;
    public long? NextChargeTime { get; private set; } = null;

    public event Action<float> OnEnergyChanged;

    private void Awake()
    {
        if (Instance) {
            Debug.Log($"[{nameof(BuilderEnergyManager)}] Another BuilderEnergyManager is on the scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        ConstructionComponent.OnGlobalConstructionStarted += OnConstructionStarted;
    }

    private void OnDisable()
    {
        ConstructionComponent.OnGlobalConstructionStarted -= OnConstructionStarted;
    }

    private void Update()
    {
        if (NextChargeTime == null) return;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        while (NextChargeTime != null && currentTime >= NextChargeTime.Value) {
            ChargeEnergy();

            if (CurrentEnergy >= 1f) {
                NextChargeTime = null;
                break;
            }

            NextChargeTime += chargeEnergyFrequency;
        }
    }

    public void Init()
    {
        Init(BuilderEnergyData.Default() ?? new BuilderEnergyData());
    }

    public void Init(BuilderEnergyData data)
    {
        if (data == null) {
            Debug.LogError($"[{nameof(BuilderEnergyManager)}] Builder Energy Data is not valid! Initializing with defaults.");
            Init();
            return;
        }

        SetEnergy(data.CurrentEnergy);

        if (CurrentEnergy >= 1f) {
            NextChargeTime = null;
            return;
        }

        if (data.NextChargeTime == null) {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            NextChargeTime = currentTime + chargeEnergyFrequency;
            return;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long targetTime = data.NextChargeTime.Value;

        if (now >= targetTime) {
            long overdueSeconds = now - targetTime;

            long additionalTicks = overdueSeconds / chargeEnergyFrequency;
            long totalTicksGained = 1 + additionalTicks;

            float energyGained = totalTicksGained * chargeEnergyPower;
            SetEnergy(CurrentEnergy + energyGained);

            if (CurrentEnergy >= 1f) {
                NextChargeTime = null;
            }
            else {
                NextChargeTime = targetTime + (totalTicksGained * chargeEnergyFrequency);
            }
        }
        else {
            NextChargeTime = targetTime;
        }
    }

    public Dictionary<string, string> GetLocalization()
    {
        var remainingTime = GetRemainingChargeTime();

        return new Dictionary<string, string>()
        {
            { "currentEnergy", CurrentEnergy > 0 ? $"<color=green>{CurrentEnergy * 100:0}%</color>" : $"<color=red>{CurrentEnergy * 100:0}%</color>" },
            { "chargePower", (chargeEnergyPower * 100).ToString("0") },
            { "chargeRemainingTime", remainingTime > 0 ? TimeFormatter.SecondsToTimer(remainingTime) : "-" },
        };
    }

    private void OnConstructionStarted(ConstructionComponent constructionComponent)
    {
        if (!ShouldApplyBonus()) return;

        constructionComponent.SetConstructionSpeedBonus(CurrentEnergy);
        constructionComponent.ApplyConstructionSpeedBonus();

        SpendEnergy();
    }

    private void ChargeEnergy()
    {
        SetEnergy(CurrentEnergy + chargeEnergyPower);
    }

    private void SpendEnergy()
    {
        SetEnergy(CurrentEnergy - energySpend);

        if (CurrentEnergy < 1f && NextChargeTime == null) {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            NextChargeTime = currentTime + chargeEnergyFrequency;
        }
    }

    private void SetEnergy(float value)
    {
        value = Mathf.Clamp01(value);

        CurrentEnergy = value;
        OnEnergyChanged?.Invoke(value);
    }

    private bool ShouldApplyBonus()
    {
        if (!buildingsLoader || !buildingsLoader.IsLoaded) return false;
        if (CurrentEnergy <= 0f) return false;

        return true;
    }

    private int GetRemainingChargeTime()
    {
        if (NextChargeTime == null) return 0;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (int)Mathf.Max(0, NextChargeTime.Value - currentTime);
    }
}