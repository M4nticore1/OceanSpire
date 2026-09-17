using UnityEngine;

public class ConnectionTypeInfoPanelController : StatInfoPanelController
{
    private TowerBuilding towerBuilding;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        towerBuilding = informationable as TowerBuilding;

        return towerBuilding != null;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        if (towerBuilding == null)
            return null;

        var definition = towerBuilding.Definition;

        if (definition == null)
            return null;

        return new StatInfoPanelData(LocalizationManager.Instance.GetLocalizedText(definition.ConnectionType.ToString().ToLower()), PlaceHolderName);
    }
}