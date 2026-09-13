using UnityEngine;

public class ConnectionTypeInfoPanelController : StatInfoPanelController
{
    private TowerBuilding towerBuilding;

    protected override bool ShouldDisplay(IInformationable informationable)
    {
        return informationable as TowerBuilding;
    }

    protected override InfoPanelData GetData(IInformationable informationable)
    {
        if (towerBuilding == null) return null;

        var definition = towerBuilding.Definition;
        if (definition == null) return null;

        //return new StatInfoPanelData(definition.ConnectionType.ToString(), placeHolderName);
        return null;
    }
}