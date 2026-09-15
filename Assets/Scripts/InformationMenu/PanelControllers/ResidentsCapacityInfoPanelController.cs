using UnityEngine;

public class ResidentsCapacityInfoPanelController : StatInfoPanelController
{
    private IResidentsCapacityProvider residentsCapacityProvider;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        residentsCapacityProvider = informationable as IResidentsCapacityProvider;
        if (residentsCapacityProvider == null) return false;

        return residentsCapacityProvider.ResidentsCapacity > 0;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StatInfoPanelData(residentsCapacityProvider.ResidentsCapacity.ToString(), PlaceHolderName);
    }
}