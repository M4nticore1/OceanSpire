using UnityEngine;

public class ResidentsCapacityInfoPanelController : StatInfoPanelController
{
    private PopulationCapacityProvider populationCapacityProvider;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        populationCapacityProvider = informationable as PopulationCapacityProvider;

        return populationCapacityProvider != null;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StatInfoPanelData(populationCapacityProvider.PopulationCapacity.ToString(), "populationCapacity");
    }
}