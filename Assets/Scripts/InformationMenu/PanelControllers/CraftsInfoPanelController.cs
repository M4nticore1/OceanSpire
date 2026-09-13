using UnityEngine;

public class CraftsInfoPanelController : InfoPanelController
{
    private IRecipeProvider craftingStation;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        craftingStation = informationable as IRecipeProvider;
        if (craftingStation == null) return false;

        return true;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new CraftsInfoPanelData(craftingStation.Crafts);
    }
}