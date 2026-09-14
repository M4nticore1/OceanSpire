using UnityEngine;

public class CraftsInfoPanelController : InfoPanelController
{
    private IRecipeProvider recipeProvider;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        recipeProvider = informationable as IRecipeProvider;
        if (recipeProvider == null) return false;

        return recipeProvider.CraftDefinitions != null;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        return new CraftsInfoPanelData(recipeProvider.CraftDefinitions);
    }
}