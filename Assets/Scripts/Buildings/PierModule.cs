using UnityEngine;

public class PierModule : BuildingModule
{
    protected override void OnBuildingInited()
    {
        PierConstruction pierConstruction = OwnedBuilding.spawnedConstruction as PierConstruction;
        if (pierConstruction) {
            int docksCount = pierConstruction.BoatDockPositions.Count;
            for (int i = 0; i < docksCount; i++) {
                if (CityManager.Instance.spawnedBoats.Count <= i) break;

                if (CityManager.Instance.spawnedBoats[i] && !CityManager.Instance.spawnedBoats[i].isDocked) {
                    CityManager.Instance.spawnedBoats[i].transform.position = pierConstruction.BoatDockPositions[i].position;
                    CityManager.Instance.spawnedBoats[i].transform.rotation = pierConstruction.BoatDockPositions[i].rotation;
                }
            }
        }
        else
            Debug.LogError(OwnedBuilding.BuildingData.BuildingName + " has no pierConstruction");
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {
        
    }

    protected override void OnEnterBuilding()
    {
        
    }

    protected override void OnExitBuilding()
    {
        
    }

    protected override void OnResidentStartWorking()
    {
        
    }

    protected override void OnResidentStopWorking()
    {
        
    }
}
