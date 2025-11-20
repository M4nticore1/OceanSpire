using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering;

public class PierBuilding : Building
{
    public List<Boat> spawnedBoats { get; private set; } = new List<Boat>();

    protected override void OnEnable()
    {
        Boat.OnBoadDestroyed += OnBoatDestroyed;
    }

    public void AddBoat(Boat boat, int dockIndex)
    {
        PierConstruction pierConstruction = constructionComponent.spawnedConstruction as PierConstruction;
        Vector3 position = pierConstruction.BoatDockPositions[spawnedBoats.Count].transform.position;
        Quaternion rotation = pierConstruction.BoatDockPositions[spawnedBoats.Count].transform.rotation;

        Boat spawnedBoat = Instantiate(boat, position, rotation);
        spawnedBoat.StartBuilding(this, dockIndex);
        spawnedBoats[dockIndex] = spawnedBoat;
    }

    protected override void FinishConstructing()
    {
        base.FinishConstructing();

        PierConstruction pierConstruction = constructionComponent.spawnedConstruction as PierConstruction;
        if (pierConstruction)
        {
            for (int i = spawnedBoats.Count; i < pierConstruction.BoatDockPositions.Count; i++)
            {
                spawnedBoats.Add(null);
            }
        }
        else
            Debug.LogError(BuildingData.BuildingName + " has no pierConstruction");
    }

    private void OnBoatDestroyed(Boat boat)
    {
        spawnedBoats[boat.dockIndex] = null;
    }
}
