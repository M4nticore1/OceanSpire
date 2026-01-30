using System.Collections.Generic;
using UnityEngine;

public class ConstructionComponent : MonoBehaviour
{
    public Building ownedBuilding { get; private set; } = null;
    private Boat ownedBoat = null;

    [Header("Main")]
    private int levelIndex => ownedBuilding ? ownedBuilding.LevelIndex : 0;
    public List<ConstructionLevelData> constructionLevelsData { get; private set; } = null;

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

    public event System.Action onBuildingStartConstructing;
    public event System.Action onBuildingFinishConstructing;
    public event System.Action onConstructionDemolished;

    private void GetComponents()
    {
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

    public void StartConstructing(int nextLevel = 0)
    {
        Debug.Log("StartConstructing");
        isUnderConstruction = true;
        if (ownedBuilding) {
            if (levelIndex == 0)
                ownedBuilding.SetLevel(nextLevel);
        }

        onBuildingStartConstructing?.Invoke();
        EventBus.Instance.InvokeConstructionPlaced(this);
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
        if (ownedBuilding) {
            ownedBuilding.SetLevel(levelIndex);;
        }

        onBuildingFinishConstructing?.Invoke();
        EventBus.Instance.InvokeConstructionBuilt(this);
    }

    public void StartUpgrading()
    {
        int level = levelIndex + 1;
        StartConstructing(level);
    }

    public void Demolish()
    {
        onConstructionDemolished?.Invoke();
        EventBus.Instance.InvokeConstructionDemolished(this);
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
        ItemData data = ItemsList.Instance.Items[lootId];
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
            ItemData data = ItemsList.Instance.Items[lootId];
            ItemInstance item = new ItemInstance(data); // The same item instance for list and dictionary.
            deliveredConstructionResources.Add(item);
            deliveredConstructionResourcesDict.Add(lootId, item);
        }

        // We can change only the list or dictionary because we use the same item instance for them.
        int amountToAdd = deliveredConstructionResourcesDict[lootId].AddAmount(amount);
        SubtractIncomingConstructionResources(lootId, amountToAdd);

        // Finish building
        ItemInstance[] resourcesToBuild = constructionLevelsData[levelIndex].ResourcesToBuild;
        if (deliveredConstructionResourcesDict[lootId].Amount >= ItemsList.Instance.GetItem(lootId, constructionLevelsData[levelIndex].ResourcesToBuild).Amount)
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
                spawnedConstruction.Build(ownedBuilding ? ownedBuilding : null);

                if (spawnedConstruction && spawnedConstruction.BuildingInteriors.Length > 0)
                {
                    interiorIndex = UnityEngine.Random.Range(0, spawnedConstruction.BuildingInteriors.Length);

                    if (interiorIndex < 0)
                        interiorIndex = UnityEngine.Random.Range(0, spawnedConstruction.BuildingInteriors.Length);

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
        BuildingAction[] buildingInteraction = spawnedConstruction.BuildingInteractions;
        if (buildingInteraction.Length > interactionPointIndex && buildingInteraction[interactionPointIndex].waypoints.Length > waypointIndex)
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
