using UnityEngine;

public class EnergyConsumptionInfoPanelController : StatInfoPanelController
{
    private IEnergyConsumptionable energyConsumptionable;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        energyConsumptionable = informationable as IEnergyConsumptionable;
        if (energyConsumptionable == null) return false;

        return true;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StatInfoPanelData(energyConsumptionable.EnergyConsumptionPerMinute.ToString(), PlaceHolderName);
    }
}