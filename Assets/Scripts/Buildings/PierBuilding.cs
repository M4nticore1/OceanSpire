using UnityEngine;
using System.Collections.Generic;

public class PierBuilding : Building
{
    [SerializeField] private List<Boat> spawnedBoats = new List<Boat>();
    public IReadOnlyList<Boat> SpawnedBoats => spawnedBoats.AsReadOnly();

    protected override void OnEnable()
    {
        Boat.OnBoadDestroyed += OnBoatDestroyed;
    }

    public void CreateBoat(Boat boat, bool isUnderConstruction = false, int? dockIndex = null, bool isDocked = true, bool isReturningToDock = false, float? health = null, float? positionX = null, float? positionZ = null, float? rotationY = null)
    {
        PierConstruction pierConstruction = constructionComponent.spawnedConstruction as PierConstruction;
        if (dockIndex == null)
        {
            for (int i = 0; i < spawnedBoats.Count; i++)
            {
                if (!spawnedBoats[i])
                {
                    dockIndex = i;
                    break;
                }
            }
        }

        Vector3 position = Vector3.zero /*pierConstruction.BoatDockPositions[dockIndex.Value].position*/;
        if (positionX != null) position.x = positionX.Value;
        if (positionZ != null) position.z = positionZ.Value;

        Quaternion rotation = Quaternion.identity;
        if (rotationY != null) rotation = Quaternion.Euler(0, rotationY.Value, 0);
        else rotation = pierConstruction.BoatDockPositions[dockIndex.Value].rotation;

        if (spawnedBoats[dockIndex.Value])
            spawnedBoats[dockIndex.Value].Demolish(false);

        Boat spawnedBoat = Instantiate(boat, position, rotation);
        spawnedBoat.Initialize(this, isUnderConstruction, dockIndex.Value, isDocked, isReturningToDock, health);
        spawnedBoats[dockIndex.Value] = spawnedBoat;
    }

    public override void FinishConstructing()
    {
        base.FinishConstructing();

        PierConstruction pierConstruction = constructionComponent.spawnedConstruction as PierConstruction;
        if (pierConstruction)
        {
            int docksCount = pierConstruction.BoatDockPositions.Count;
            for (int i = 0; i < docksCount; i++)
            {
                if (i < spawnedBoats.Count)
                {
                    if (spawnedBoats[i] && !spawnedBoats[i].isMoving)
                    {
                        spawnedBoats[i].transform.position = pierConstruction.BoatDockPositions[i].position;
                        spawnedBoats[i].transform.rotation = pierConstruction.BoatDockPositions[i].rotation;
                    }
                }
                else
                {
                    spawnedBoats.Add(null);
                }
            }
        }
        else
            Debug.LogError(BuildingData.BuildingName + " has no pierConstruction");
    }

    private void OnBoatDestroyed(Boat boat)
    {
        spawnedBoats[boat.dockIndex] = null;
    }

    public Boat GetBoatByIndex(int index)
    {
        for (int i = 0; i < SpawnedBoats.Count; i++)
        {
            if (SpawnedBoats[i])
                return SpawnedBoats[i];
        }

        return null;
    }
}
