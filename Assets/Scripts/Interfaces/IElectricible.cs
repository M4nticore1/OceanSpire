using UnityEngine;

public interface IElectricible
{
    public bool IsUnderEnergyShortage { get; }
    public float EnergyConsumptionPerMinute { get; }
    public void SetUnderEnergyShortage(bool value);
    public bool ShouldSpendElectricity();
}
