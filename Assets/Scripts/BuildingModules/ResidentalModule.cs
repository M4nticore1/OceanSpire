using UnityEngine;

public class ResidentalModule : BuildingModule, PopulationCapacityProvider
{
    public int PopulationCapacity {
        get {
            var storageModule = GetComponent<StorageModule>();
            if (storageModule == null) return 0;

            var storageLevelData = storageModule.StorageLevelData;
            if (storageLevelData == null) return 0;

            var populationStack = storageLevelData.GetStack(ItemStackEnum.Population);
            if (populationStack == null) return 0;

            return populationStack.Amount;
        }
    }
}
