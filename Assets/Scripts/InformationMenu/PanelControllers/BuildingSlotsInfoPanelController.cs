using UnityEngine;

public class BuildingSlotsInfoPanelController : StatInfoPanelController
{
    private IBuildingSlotsProvider buildingSlotsProvider;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        buildingSlotsProvider = informationable as IBuildingSlotsProvider;
        if (buildingSlotsProvider == null) return false;

        return buildingSlotsProvider.BuildingSlotsCount > 0;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StatInfoPanelData(buildingSlotsProvider.BuildingSlotsCount.ToString(), PlaceHolderName);
    }
}