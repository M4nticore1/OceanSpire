using UnityEngine;

public interface IElectricible
{
    public bool IsUnderEnergyShortage {  get; }
    public void SetUnderEnergyShortage(bool value);
    public float GetElectricityConsumptionPerMinute();
    public bool ShouldSpendElectricity();
}
