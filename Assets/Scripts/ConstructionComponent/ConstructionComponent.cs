using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    public LevelComponent levelComponent { get; private set; } = null;

    [Header("Main")]
    [SerializeField] private List<ConstructionLevelData> constructionLevelsData = new List<ConstructionLevelData>();
    public List<ConstructionLevelData> ConstructionLevelsData => constructionLevelsData;

    [Header("Construction")]
    public bool isRuined { get; private set; } = false;
    public bool isUnderConstruction { get; private set; } = false;
    public BuildingConstruction spawnedBuildingConstruction = null;

    public List<ItemInstance> incomingConstructionResources { get; private set; } = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> incomingConstructionResourcesDict { get; private set; } = new Dictionary<int, ItemInstance>();
    public List<ItemInstance> deliveredConstructionResources { get; private set; } = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> deliveredConstructionResourcesDict { get; private set; } = new Dictionary<int, ItemInstance>();

    protected GameObject spawedBuildingInterior { get; private set; } = null;
    public int interiorIndex { get; private set; } = 0;

    public static event System.Action<ConstructionComponent> onAnyConstructionStartConstructing;
    public event System.Action onBuildingStartConstructing;
    public static event System.Action<ConstructionComponent> onAnyConstructionFinishConstructing;
    public event System.Action onBuildingFinishConstructing;

    public static event System.Action<ConstructionComponent> onAnyConstructionDemolished;
    public event System.Action onConstructionDemolished;

    public virtual void InitializeBuilding(BuildingPlace buildingPlace)
    {
        levelComponent = GetComponent<LevelComponent>();
    }

    public virtual void Place(BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex)
    {
        InitializeBuilding(buildingPlace);
        isUnderConstruction = requiresConstruction;
        this.interiorIndex = interiorIndex;

        if (isUnderConstruction)
            StartBuilding(levelIndex);
        else
            Build(levelIndex, interiorIndex);
    }

    //private IEnumerator PlaceCoroutine(BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex)
    //{
    //    yield return new WaitForEndOfFrame();

    //    InitializeBuilding(buildingPlace);

    //    if (isUnderConstruction)
    //        StartBuilding(levelIndex);
    //    else
    //        Build(levelIndex, interiorIndex);

    //    this.levelIndex = levelIndex;
    //    isUnderConstruction = requiresConstruction;
    //    this.interiorIndex = interiorIndex;
    //}

    public void StartBuilding(int nextLevelIndex)
    {
        isUnderConstruction = true;

        //UpdateBuildingConstruction(nextLevelIndex);

        onAnyConstructionStartConstructing?.Invoke(this);
        onBuildingStartConstructing?.Invoke();
    }

    public void FinishBuilding()
    {
        if (isRuined)
            isRuined = false;
        else if (isUnderConstruction)
            isUnderConstruction = false;
        else
            levelComponent.levelIndex++;

        interiorIndex = UnityEngine.Random.Range(0, spawnedBuildingConstruction.buildingInteriors.Count);

        Build(levelComponent.levelIndex, interiorIndex);
    }

    protected virtual void Build(int levelIndex, int interiorIndex)
    {
        //UpdateBuildingConstruction(levelIndex);

        if (spawnedBuildingConstruction && spawnedBuildingConstruction.buildingInteriors.Count > 0)
        {
            if (interiorIndex < 0)
                interiorIndex = UnityEngine.Random.Range(0, spawnedBuildingConstruction.buildingInteriors.Count);

            spawedBuildingInterior = Instantiate(spawnedBuildingConstruction.buildingInteriors[interiorIndex], transform);
        }

        onAnyConstructionFinishConstructing?.Invoke(this);
        onBuildingFinishConstructing?.Invoke();
    }

    //protected virtual void UpdateBuildingConstruction(int levelIndex)
    //{
    //    BuildConstruction(levelIndex);
    //}

    //protected virtual void BuildConstruction(int levelIndex)
    //{
    //    if (spawnedBuildingConstruction)
    //    {
    //        Destroy(spawnedBuildingConstruction.gameObject);
    //        Destroy(spawedBuildingInterior);
    //    }

    //    //if (ConstructionLevelsData[levelIndex] as RoomConstructionLevelData)
    //    //{
    //    //    RoomConstructionLevelData levelData = ConstructionLevelsData[levelIndex] as RoomConstructionLevelData;

    //    //    if (levelData.ConstructionStraight)
    //    //        spawnedBuildingConstruction = Instantiate(levelData.ConstructionStraight, transform);
    //    //}
    //    //else if (ConstructionLevelsData[levelIndex] as BuildingConstructionLevelData)
    //    //{
    //    //    BuildingConstructionLevelData levelData = ConstructionLevelsData[levelIndex] as BuildingConstructionLevelData;

    //    //    if (levelData.ConstructionStraight)
    //    //        spawnedBuildingConstruction = Instantiate(levelData.ConstructionStraight, transform);
    //    //}

    //    //if (spawnedBuildingConstruction)
    //        //spawnedBuildingConstruction.Build();
    //}

    public void Demolish()
    {
        onAnyConstructionDemolished?.Invoke(this);
        Destroy(gameObject);
    }

    // Resources
    public void AddIncomingConstructionResources(int itemId, int itemAmount)
    {
        AddIncomingConstructionResources_Internal(itemId, itemAmount);
    }

    public void AddIncomingConstructionResources(ItemInstance item)
    {
        AddIncomingConstructionResources_Internal(item.ItemData.ItemId, item.Amount);
    }

    private void AddIncomingConstructionResources_Internal(int itemId, int amount)
    {
        if (!incomingConstructionResourcesDict.ContainsKey(itemId))
        {
            ItemInstance item = new ItemInstance(itemId); // The same item instance for list and dictionary.
            incomingConstructionResources.Add(item);
            incomingConstructionResourcesDict.Add(itemId, item);
        }

        // We can change only the list or dictionary because we use the same item instance for them.
        incomingConstructionResourcesDict[itemId].AddAmount(amount);
    }

    public void SubtractIncomingConstructionResources(int itemId, int itemAmount)
    {
        SubtractIncomingConstructionResources_Internal(itemId, itemAmount);
    }

    public void SubtractIncomingConstructionResources(ItemInstance item)
    {
        SubtractIncomingConstructionResources_Internal(item.ItemData.ItemId, item.Amount);
    }

    private void SubtractIncomingConstructionResources_Internal(int itemId, int amount)
    {
        if (incomingConstructionResourcesDict.ContainsKey(itemId))
        {
            // We can change only the list or dictionary because we use the same item instance for them.
            incomingConstructionResourcesDict[itemId].SubtractAmount(amount);
        }
    }

    public int AddConstructionResources(ItemInstance item)
    {
        return AddConstructionResources_Internal(item.ItemData.ItemId, item.Amount);
    }

    public int AddConstructionResources(int itemId, int amount)
    {
        return AddConstructionResources_Internal(itemId, amount);
    }

    private int AddConstructionResources_Internal(int itemId, int amount)
    {
        if (!deliveredConstructionResourcesDict.ContainsKey(itemId))
        {
            ItemInstance item = new ItemInstance(itemId); // The same item instance for list and dictionary.
            deliveredConstructionResources.Add(item);
            deliveredConstructionResourcesDict.Add(itemId, item);
        }

        // We can change only the list or dictionary because we use the same item instance for them.
        int amountToAdd = deliveredConstructionResourcesDict[itemId].AddAmount(amount);
        SubtractIncomingConstructionResources(itemId, amountToAdd);

        // Finish building
        List<ItemInstance> resourcesToBuild = constructionLevelsData[levelComponent.levelIndex].ResourcesToBuild;
        if (deliveredConstructionResourcesDict[itemId].Amount >= ItemDatabase.GetItem(itemId, constructionLevelsData[levelComponent.levelIndex].ResourcesToBuild).Amount)
        {
            foreach (var item in resourcesToBuild)
                if (item.Amount < 0)
                    return amountToAdd;
            FinishBuilding();
        }
        return amountToAdd;
    }

    protected void InvokeStartConstructing(ConstructionComponent construction)
    {
        onAnyConstructionStartConstructing?.Invoke(construction);
        onBuildingStartConstructing?.Invoke();
    }

    protected void InvokeFinishConstructing(ConstructionComponent construction)
    {
        onAnyConstructionFinishConstructing?.Invoke(construction);
        onBuildingFinishConstructing?.Invoke();
    }
}
