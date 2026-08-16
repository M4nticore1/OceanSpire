using UnityEngine;

public interface IElectricible
{
    public float GetElectricityConsumption();
    public bool ShouldSpendElectricity();
}
