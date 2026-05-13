using UnityEngine;

public static class ConstructionFactory
{
    public static BuildingConstruction CreateConstruction(BuildingConstruction constructionToSpawn, Transform transform, BuildingConstructionData data)
    {
        var spawnedConstruction = Object.Instantiate(constructionToSpawn, transform);
        spawnedConstruction.Init(data);

        return spawnedConstruction;
    }

    public static ElevatorCabinConstruction CreateConstruction(ElevatorCabinConstruction constructionToSpawn, Transform transform, BuildingConstructionData data)
    {
        var spawnedCabin = Object.Instantiate(constructionToSpawn, transform.position, transform.rotation);
        spawnedCabin.Init(data);

        return spawnedCabin;
    }
}
