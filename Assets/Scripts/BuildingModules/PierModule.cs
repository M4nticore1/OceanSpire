using UnityEngine;

public class PierModule : BuildingModule
{
    public PierConstruction PierConstruction => OwnedBuilding.spawnedConstruction as PierConstruction;
}
