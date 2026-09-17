using UnityEngine;

public class WanderersCooldownInfoPanelController : StatInfoPanelController
{
    IWanderersCooldownProvider wanderersCooldownProvider;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        wanderersCooldownProvider = informationable as IWanderersCooldownProvider;
        if (wanderersCooldownProvider == null) return false;

        return wanderersCooldownProvider.WanderersCooldownReduction > 0;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new StatInfoPanelData((wanderersCooldownProvider.WanderersCooldownReduction * 100).ToString(), PlaceHolderName);
    }
}