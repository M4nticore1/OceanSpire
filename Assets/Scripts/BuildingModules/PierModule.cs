using UnityEngine;

public class PierModule : BuildingModule
{
    public PierConstruction PierConstruction => OwnedBuilding.spawnedConstruction as PierConstruction;

    protected override void Subscribe()
    {

    }

    protected override void Unsubscribe()
    {

    }
}
