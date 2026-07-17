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
            Debug.Log("Another ConstructionEnergyManager is on the scene!");
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
        if (currentTime < NextChargeTime) return;

        ChargeEnergy();
        UpdateNextChargeTime();
    }

    public void Init()
    {
        Init(BuilderEnergyData.Default() ?? new BuilderEnergyData());
    }

    public void Init(BuilderEnergyData data)
    {
        if (data == null) {
            Debug.LogError("constructionEnergyData is not valid");
            Init();
            return;
        }

        SetEnergy(data.CurrentEnergy);

        if (CurrentEnergy < 1f) {
            if (data.NextChargeTime != null) {
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long nextChargeTime = data.NextChargeTime.Value;

                if (currentTime >= nextChargeTime) {
                    long overdueTime = currentTime - nextChargeTime;
                    long additionalTicks = overdueTime / chargeEnergyFrequency;
                    long totalTicksGained = 1 + additionalTicks;

                    float energyGained = totalTicksGained * chargeEnergyPower;
                    SetEnergy(Mathf.Min(1f, CurrentEnergy + energyGained));

                    if (CurrentEnergy >= 1f) {
                        NextChargeTime = null;
                    }
                    else {
                        long remainingSeconds = overdueTime % chargeEnergyFrequency;
                        NextChargeTime = currentTime + (chargeEnergyFrequency - remainingSeconds);
                    }
                }
                else {
                    NextChargeTime = nextChargeTime;
                }
            }
            else {
                Debug.LogError("Energy is not full, but full charge time is not valid");
                SetEnergy(1f);
            }
        }
    }

    public Dictionary<string, string> GetLocalization()
    {
        var remainingTime = GetRemainingChargeTime();

        return new Dictionary<string, string>()
        {
            { "currentEnergy", CurrentEnergy > 0 ? $"<color=green>{CurrentEnergy * 100}%</color>" : $"{CurrentEnergy * 100}%" },
            { "chargePower", (chargeEnergyPower * 100).ToString() },
            { "chargeRemainingTime", remainingTime > 0 ? TimeFormatter.SecondsToTimer(GetRemainingChargeTime()) : "-" },
        };
    }

    private void OnConstructionStarted(ConstructionComponent constructionComponent)
    {
        if (!ShouldApplyBonus()) return;

        constructionComponent.SetConstructionSpeedBonus(CurrentEnergy);
        constructionComponent.ApplyConstructionSpeedBonus();

        SpendEnergy();
        UpdateNextChargeTime();
    }

    private void ChargeEnergy()
    {
        SetEnergy(CurrentEnergy + chargeEnergyPower);
    }

    private void SpendEnergy()
    {
        SetEnergy(CurrentEnergy - energySpend);
    }

    private void SetEnergy(float value)
    {
        value = Mathf.Clamp01(value);

        CurrentEnergy = value;
        OnEnergyChanged?.Invoke(value);
    }

    private void UpdateNextChargeTime()
    {
        if (CurrentEnergy >= 1f) {
            NextChargeTime = null;
        }
        else if (NextChargeTime == null) {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            NextChargeTime = currentTime + chargeEnergyFrequency;
        }
    }

    private bool ShouldApplyBonus()
    {
        if (!buildingsLoader.IsLoaded) return false;
        if (CurrentEnergy <= 0f) return false;

        return true;
    }

    private int GetRemainingChargeTime()
    {
        if (NextChargeTime == null) return 0;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (int)(NextChargeTime.Value - currentTime);
    }
}