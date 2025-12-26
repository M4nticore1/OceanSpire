using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    private GameManager gameManager = null;
    private Building ownedBuilding = null;
    private Boat ownedBoat = null;
    public LevelComponent levelComponent { get; private set; } = null;

    [Header("Main")]
    public List<ConstructionLevelData> constructionLevelsData { get; private set; } = null;
    //public List<ConstructionLevelData> ConstructionLevelsData => constructionLevelsData;

    [Header("Construction")]
    [SerializeField] private BuildingConstruction spawnedConstruction = null;
    public BuildingConstruction SpawnedConstruction => spawnedConstruction;
    public bool isRuined { get; private set; } = false;
    public bool isUnderConstruction { get; private set; } = false;

    public List<ItemInstance> incomingConstructionResources { get; private set; } = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> incomingConstructionResourcesDict { get; private set; } = new Dictionary<int, ItemInstance>();
    public List<ItemInstance> deliveredConstructionResources { get; private set; } = new List<ItemInstance>();
    public Dictionary<int, ItemInstance> deliveredConstructionResourcesDict { get; private set; } = new Dictionary<int, ItemInstance>();

    protected GameObject spawedBuildingInterior { get; private set; } = null;
    public int interiorIndex { get; private set; } = -1;

    private bool IsInitialized = false;

    public static event System.Action<ConstructionComponent> onAnyConstructionStartConstructing;
    public event System.Action onBuildingStartConstructing;
    public static event System.Action<ConstructionComponent> onAnyConstructionFinishConstructing;
    public event System.Action onBuildingFinishConstructing;

    public static event System.Action<ConstructionComponent> onAnyConstructionDemolished;
    public event System.Action onConstructionDemolished;

    private void GetComponents()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        levelComponent = GetComponent<LevelComponent>();
        ownedBuilding = GetComponent<Building>();
        ownedBoat = GetComponent<Boat>();
        if (ownedBuilding)
            constructionLevelsData = ownedBuilding.ConstructionLevelsData;
    }

    public void InitializeConstruction(bool requiresConstruction = false, int levelIndex = 0)
    {
        if (IsInitialized) return;

        GetComponents();

        isUnderConstruction = requiresConstruction;

        if (isUnderConstruction)
            StartConstructing(levelIndex);
        else
            FinishConstructing(levelIndex);

        IsInitialized = true;
    }

    //public void Place()
    //{
    //    if (isUnderConstruction)
    //        StartConstructing();
    //    else
    //        FinishConstructing();

    //    IsInitialized = true;
    //}

    public void StartConstructing(int nextLevel = 0)
    {
        Debug.Log("StartConstructing");
        isUnderConstruction = true;
        if (levelComponent && levelComponent.LevelIndex == 0)
            levelComponent.LevelIndex = nextLevel;

        onAnyConstructionStartConstructing?.Invoke(this);
        onBuildingStartConstructing?.Invoke();
    }

    public void FinishConstructing(int nextLevel = 0)
    {
        if (isRuined)
            isRuined = false;
        else if (isUnderConstruction)
            isUnderConstruction = false;

        Build(nextLevel);
    }

    protected void Build(int levelIndex = 0)
    {
        if (levelComponent)
            levelComponent.LevelIndex = levelIndex;

        if (levelComponent)
            levelComponent.LevelIndex = levelIndex;

        onAnyConstructionFinishConstructing?.Invoke(this);
        onBuildingFinishConstructing?.Invoke();
    }

    public void StartUpgrading()
    {
        if (levelComponent)
        {
            int level = levelComponent.LevelIndex + 1;
            StartConstructing(level);
        }
    }

    public void StartDemolishing()
    {
        onAnyConstructionDemolished?.Invoke(this);
        Destroy(gameObject);
    }

    // Resources
    public void AddIncomingConstructionResources(int itemId, int amount)
    {
        AddIncomingConstructionResources_Internal(itemId, amount);
    }

    public void AddIncomingConstructionResources(ItemInstance item)
    {
        AddIncomingConstructionResources_Internal(item.ItemData.ItemId, item.Amount);
    }

    private void AddIncomingConstructionResources_Internal(int lootId, int amount)
    {
        ItemData data = gameManager.LootList.lootById[lootId];
        ItemInstance loot = new ItemInstance(data, amount);
        if (!incomingConstructionResourcesDict.ContainsKey(lootId))
        {
            incomingConstructionResources.Add(loot);
            incomingConstructionResourcesDict.Add(lootId, loot);
        }

        // We can change only the list or dictionary because we use the same item instance for them.
        incomingConstructionResourcesDict[lootId].AddAmount(amount);
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

    public int AddConstructionResources(int itemId, int amount)
    {
        return AddConstructionResources_Internal(itemId, amount);
    }

    public int AddConstructionResources(ItemInstance item)
    {
        return AddConstructionResources_Internal(item.ItemData.ItemId, item.Amount);
    }

    private int AddConstructionResources_Internal(int lootId, int amount)
    {
        if (!deliveredConstructionResourcesDict.ContainsKey(lootId))
        {
            ItemData data = gameManager.LootList.lootById[lootId];
            ItemInstance item = new ItemInstance(data); // The same item instance for list and dictionary.
            deliveredConstructionResources.Add(item);
            deliveredConstructionResourcesDict.Add(lootId, item);
        }

        // We can change only the list or dictionary because we use the same item instance for them.
        int amountToAdd = deliveredConstructionResourcesDict[lootId].AddAmount(amount);
        SubtractIncomingConstructionResources(lootId, amountToAdd);

        // Finish building
        List<ItemInstance> resourcesToBuild = constructionLevelsData[levelComponent.LevelIndex].ResourcesToBuild;
        if (deliveredConstructionResourcesDict[lootId].Amount >= gameManager.LootList.GetItem(lootId, constructionLevelsData[levelComponent.LevelIndex].ResourcesToBuild).Amount)
        {
            foreach (var item in resourcesToBuild)
                if (item.Amount < 0)
                    return amountToAdd;
            FinishConstructing();
        }
        return amountToAdd;
    }

    public void BuildConstruction(BuildingConstruction buildingConstruction)
    {
        if (buildingConstruction)
        {
            if (spawnedConstruction)
            {
                Destroy(spawnedConstruction.gameObject);
                spawnedConstruction = null;
            }

            if (!spawnedConstruction)
            {
                BuildingConstruction construction = Instantiate(buildingConstruction, gameObject.transform);
                spawnedConstruction = construction;
                spawnedConstruction.Build();

                if (spawnedConstruction && spawnedConstruction.BuildingInteriors.Count > 0)
                {
                    interiorIndex = UnityEngine.Random.Range(0, spawnedConstruction.BuildingInteriors.Count);

                    if (interiorIndex < 0)
                        interiorIndex = UnityEngine.Random.Range(0, spawnedConstruction.BuildingInteriors.Count);

                    spawedBuildingInterior = Instantiate(spawnedConstruction.BuildingInteriors[interiorIndex], transform);
                }
            }
        }
        else
            Debug.LogError("buildingConstruction is NULL");
    }

    //public Vector3 GetInteractionPosition()
    //{
    //    List<BuildingAction> buildingInteraction = spawnedConstruction.BuildingInteractions;
    //    if (buildingInteraction.Count > 0 && buildingInteraction[0].waypoints.Count > 0)
    //        return GetInteractionPosition(0, 0);
    //    else
    //        return transform.position;
    //}

    public Vector3 GetInteractionPosition(int interactionPointIndex, int waypointIndex = 0)
    {
        List<BuildingAction> buildingInteraction = spawnedConstruction.BuildingInteractions;
        if (buildingInteraction.Count > interactionPointIndex && buildingInteraction[interactionPointIndex].waypoints.Count > waypointIndex)
            return buildingInteraction[interactionPointIndex].waypoints[waypointIndex].position;
        else
            return transform.position;
    }

    public Vector3 GetPickupItemPointPosition()
    {
        if (spawnedConstruction.collectItemPoints.Count > 0)
            return spawnedConstruction.collectItemPoints[0].position;
        else
            return transform.position;
    }
}
