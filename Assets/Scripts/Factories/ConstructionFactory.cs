using UnityEngine;

public static class ConstructionFactory
{
    public static BuildingConstruction CreateConstruction(BuildingConstruction constructionToSpawn, Building ownedBuilding)
    {
        BuildingConstruction spawnedConstruction = Object.Instantiate(constructionToSpawn, ownedBuilding.transform);
        spawnedConstruction.Init(ownedBuilding);
        return spawnedConstruction;
    }

    public static ElevatorCabinConstruction CreateConstruction(ElevatorCabinConstruction constructionToSpawn, Building ownedBuilding)
    {
        ElevatorCabinConstruction spawnedCabin = Object.Instantiate(constructionToSpawn, ownedBuilding.transform.position, ownedBuilding.transform.rotation);
        spawnedCabin.Init(ownedBuilding);
        return spawnedCabin;
    }
}
