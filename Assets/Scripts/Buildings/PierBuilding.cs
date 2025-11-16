using UnityEngine;
using System.Collections.Generic;

public class PierBuilding : EnvironmetBuilding
{
    public List<Boat> spawnedBoats { get; private set; } = new List<Boat>();

    public void OnEnable()
    {
        Boat.OnBoadDestroyed += OnBoatDestroyed;
    }

    public void AddBoat(Boat boat, int dockIndex)
    {
        PierConstruction pierConstruction = constructionComponent.spawnedBuildingConstruction as PierConstruction;
        Vector3 position = pierConstruction.BoatDockPositions[spawnedBoats.Count].transform.position;
        Quaternion rotation = pierConstruction.BoatDockPositions[spawnedBoats.Count].transform.rotation;

        Boat spawnedBoat = Instantiate(boat, position, rotation);
        spawnedBoat.StartBuilding(this, dockIndex);
        spawnedBoats[dockIndex] = spawnedBoat;
    }

    protected override void OnBuildingFinishConstructing()
    {
        base.OnBuildingFinishConstructing();

        PierConstruction pierConstruction = constructionComponent.spawnedBuildingConstruction as PierConstruction;
        for (int i = spawnedBoats.Count; i < pierConstruction.BoatDockPositions.Count; i++)
        {
            spawnedBoats.Add(null);
        }
    }

    private void OnBoatDestroyed(Boat boat)
    {
        spawnedBoats[boat.dockIndex] = null;
    }
}
