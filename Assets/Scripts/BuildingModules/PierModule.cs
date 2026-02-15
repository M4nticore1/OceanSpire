using UnityEngine;

public class PierModule : BuildingModule
{
    public PierConstruction PierConstruction => OwnedBuilding.spawnedConstruction as PierConstruction;

    protected override void OnInit()
    {

    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {
        
    }

    protected override void OnEnterBuilding(EntityCityNavigator navigator)
    {
        
    }

    protected override void OnExitBuilding(EntityCityNavigator navigator)
    {
        
    }
}
