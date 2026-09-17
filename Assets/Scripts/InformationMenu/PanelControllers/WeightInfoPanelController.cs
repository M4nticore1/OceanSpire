using UnityEngine;

public class WeightInfoPanelController : StatInfoPanelController
{
    public IWeightable weightable;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        weightable = informationable as IWeightable;

        return weightable != null;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StatInfoPanelData(weightable.Weight.ToString(), PlaceHolderName);
    }
}