using UnityEngine;

public interface IElectricible
{
    public float GetElectricityConsumptionPerMinute();
    public bool ShouldSpendElectricity();
}
