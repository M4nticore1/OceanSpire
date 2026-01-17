using UnityEngine;

public class PierBuilding : Building
{
    //public void CreateBoat(Boat boat, bool isUnderConstruction = false, int? dockIndex = null, bool isFloating = false, bool isReturningToDock = false, float? health = null, float? positionX = null, float? positionZ = null, float? rotationY = null)
    //{
    //    PierConstruction pierConstruction = constructionComponent.SpawnedConstruction as PierConstruction;
    //    if (dockIndex == null) {
    //        for (int i = 0; i < spawnedBoats.Count; i++) {
    //            if (!spawnedBoats[i]) {
    //                dockIndex = i;
    //                break;
    //            }
    //        }
    //    }

    //    Vector3 position = Vector3.zero /*pierConstruction.BoatDockPositions[dockIndex.Value].position*/;
    //    if (positionX != null) position.x = positionX.Value;
    //    if (positionZ != null) position.z = positionZ.Value;

    //    Quaternion rotation = Quaternion.identity;
    //    if (rotationY != null) rotation = Quaternion.Euler(0, rotationY.Value, 0);
    //    else rotation = pierConstruction.BoatDockPositions[dockIndex.Value].rotation;

    //    if (spawnedBoats[dockIndex.Value])
    //        spawnedBoats[dockIndex.Value].Demolish(false);

    //    Boat spawnedBoat = Instantiate(boat, position, rotation);
    //    spawnedBoat.Initialize(this, isUnderConstruction, dockIndex.Value, isFloating, isReturningToDock, health);
    //    spawnedBoats[dockIndex.Value] = spawnedBoat;
    //}

    public override void FinishConstructing()
    {
        base.FinishConstructing();

        PierConstruction pierConstruction = constructionComponent.SpawnedConstruction as PierConstruction;
        if (pierConstruction) {
            int docksCount = pierConstruction.BoatDockPositions.Count;
            for (int i = 0; i < docksCount; i++) {
                if (GameManager.Instance.spawnedBoats.Count <= i) break;

                if (GameManager.Instance.spawnedBoats[i] && !GameManager.Instance.spawnedBoats[i].isFloating) {
                    GameManager.Instance.spawnedBoats[i].transform.position = pierConstruction.BoatDockPositions[i].position;
                    GameManager.Instance.spawnedBoats[i].transform.rotation = pierConstruction.BoatDockPositions[i].rotation;
                }
            }
        }
        else
            Debug.LogError(BuildingData.BuildingName + " has no pierConstruction");
    }
}
