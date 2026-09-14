using UnityEngine;

public class EnergyConsumptionInfoPanelController : StatInfoPanelController
{
    private IElectricible energyConsumptionable;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        energyConsumptionable = informationable as IElectricible;
        if (energyConsumptionable == null) return false;

        return energyConsumptionable.EnergyConsumptionPerMinute > 0;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StatInfoPanelData(energyConsumptionable.EnergyConsumptionPerMinute.ToString(), PlaceHolderName);
    }
}