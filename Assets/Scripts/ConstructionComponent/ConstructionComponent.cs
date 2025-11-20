using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    private Building ownedBuilding = null;
    private Boat ownedBoat = null;
    public LevelComponent levelComponent { get; private set; } = null;

    [Header("Main")]
    public List<ConstructionLevelData> constructionLevelsData { get; private set; } = null;
    //public List<ConstructionLevelData> ConstructionLevelsData => constructionLevelsData;

    [Header("Construction")]
    public bool isRuined { get; private set; } = false;
    public bool isUnderConstruction { get; private set; } = false;
    public BuildingConstruction spawnedConstruction { get; private set; } = null;

    public List<ItemInstance> incomingConstructionResources { get; private set; } = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> incomingConstructionResourcesDict { get; private set; } = new Dictionary<int, ItemInstance>();
    public List<ItemInstance> deliveredConstructionResources { get; private set; } = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> deliveredConstructionResourcesDict { get; private set; } = new Dictionary<int, ItemInstance>();

    protected GameObject spawedBuildingInterior { get; private set; } = null;
    public int interiorIndex { get; private set; } = 0;

    private bool isPlaced = false;

    public static event System.Action<ConstructionComponent> onAnyConstructionStartConstructing;
    public event System.Action onBuildingStartConstructing;
    public static event System.Action<ConstructionComponent> onAnyConstructionFinishConstructing;
    public event System.Action onBuildingFinishConstructing;

    public static event System.Action<ConstructionComponent> onAnyConstructionDemolished;
    public event System.Action onConstructionDemolished;

    private void Awake()
    {
        levelComponent = GetComponent<LevelComponent>();
        ownedBuilding = GetComponent<Building>();
        ownedBoat = GetComponent<Boat>();
        if (ownedBuilding)
            constructionLevelsData = ownedBuilding.ConstructionLevelsData;
        //else if (boat)
            //constructionLevelsData = boat.;
    }

    private void Start()
    {
        
    }

    public void InitializeConstruction(int levelIndex, bool requiresConstruction)
    {
        levelComponent = GetComponent<LevelComponent>();

        isUnderConstruction = requiresConstruction;
        if (levelComponent)
            levelComponent.LevelIndex = levelIndex;
    }

    public void Place()
    {
        isPlaced = true;
    }

    public void StartConstructing()
    {
        isUnderConstruction = true;

        onAnyConstructionStartConstructing?.Invoke(this);
        onBuildingStartConstructing?.Invoke();
    }

    public void FinishBuilding()
    {
        if (isRuined)
            isRuined = false;
        else if (isUnderConstruction)
            isUnderConstruction = false;

        interiorIndex = UnityEngine.Random.Range(0, spawnedConstruction.buildingInteriors.Count);

        Build(levelComponent.LevelIndex + 1, interiorIndex);
    }

    protected void Build(int levelIndex, int interiorIndex)
    {
        if (spawnedConstruction && spawnedConstruction.buildingInteriors.Count > 0)
        {
            if (interiorIndex < 0)
                interiorIndex = UnityEngine.Random.Range(0, spawnedConstruction.buildingInteriors.Count);

            spawedBuildingInterior = Instantiate(spawnedConstruction.buildingInteriors[interiorIndex], transform);
        }

        if (levelComponent)
            levelComponent.LevelIndex = levelIndex;

        onAnyConstructionFinishConstructing?.Invoke(this);
        onBuildingFinishConstructing?.Invoke();
    }

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
        List<ItemInstance> resourcesToBuild = constructionLevelsData[levelComponent.LevelIndex].ResourcesToBuild;
        if (deliveredConstructionResourcesDict[itemId].Amount >= ItemDatabase.GetItem(itemId, constructionLevelsData[levelComponent.LevelIndex].ResourcesToBuild).Amount)
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

    public void BuildConstruction(BuildingConstruction buildingConstruction)
    {
        BuildingConstruction construction = Instantiate(buildingConstruction, gameObject.transform);
        spawnedConstruction = construction;
        spawnedConstruction.Build();
    }

    //public void SetConstruction(BuildingConstruction construction)
    //{
    //    spawnedConstruction = construction;
    //    spawnedConstruction.Build();
    //}
}
