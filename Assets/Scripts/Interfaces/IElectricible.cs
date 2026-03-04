using UnityEngine;

public interface IElectricible
{
    public float ElectricityConsumption { get; }
    public float GetElectricityConsumption();
    public bool CanSpendElectricity();
}
