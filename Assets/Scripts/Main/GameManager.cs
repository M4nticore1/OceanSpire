using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static BuildingData;

[System.Serializable]
public struct ResourceStack
{
    public ItemData resource;
    public int amount;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static CityManager cityManager;

    [Header("Buildings")]
    public List<Building> buildingPrefabs = new List<Building>();
    public const float demolitionResourceRefundRate = 0.2f;

    // Items
    [Header("Items")]
    public List<ItemData> itemsData = new List<ItemData>();
    //[HideInInspector] public List<ItemInstance> items = new List<ItemInstance>();

    [Header("NPC")]
    public Resident residentPrefab = null;

    // Wind
    [HideInInspector] public Vector2 windDirection = Vector2.zero;
    [HideInInspector] public float windRotation = 0;
    private Vector2 newWindDirection = Vector2.zero;

    public const float windSpeed = 5.0f;

    private const float windChangingSpeed = 0.05f;

    private float windDirectionChangeRate = 300.0f;
    private float windDirectionChangeTime = 0.0f;

    public const float autoSaveFrequency = 1;

    public bool hasSavedData = false;

    private void Awake()
    {
        cityManager = FindAnyObjectByType<CityManager>();

        //buildingPrefabs.Sort((a, b) => a.buildingData.buildingId.CompareTo(b.buildingData.buildingId));
    }

    private void Start()
    {
        ChangeWind();
        windDirection = newWindDirection;
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {

    }

    private void Update()
    {
        ChangingWind();
    }

    private void ChangingWind()
    {
        if (Time.time > windDirectionChangeTime + windDirectionChangeRate)
        {
            ChangeWind();
        }

        windDirection = math.lerp(windDirection, newWindDirection, windChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        float xAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        float yAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        newWindDirection = new Vector2(xAxis, yAxis).normalized;

        windDirectionChangeTime = Time.time;
    }

    public static int GetItemIndexByIdName(List<ItemData> itemsList, string idName)
    {
        int id = 0;

        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i].itemIdName == idName)
            {
                id = i;
                return id;
            }
        }

        return -1;
    }

    public static int GetItemIndexById(List<ItemData> itemsList, int id)
    {
        if ((int)itemsList[id].itemId == id)
        {
            return id;
        }
        else
        {
            int currentId = 0;

            for (int i = 0; i < itemsList.Count; i++)
            {
                if ((int)itemsList[i].itemId == id)
                {
                    currentId = i;
                    return currentId;
                }
            }

            return -1;
        }
    }

    public Building GetBuildingPrefabById(int buildingId)
    {
        for (int i = 0; i < buildingPrefabs.Count; i++)
        {
            if ((int)buildingPrefabs[i].buildingData.buildingId == buildingId)
            {
                return buildingPrefabs[i];
            }
        }

        return null;
    }

    public Building GetBuildingPrefabByIdName(string buildingIdName)
    {
        for (int i = 0; i < buildingPrefabs.Count; i++)
        {
            if (buildingPrefabs[i].buildingData.buildingIdName == buildingIdName)
            {
                return buildingPrefabs[i];
            }
        }

        return null;
    }
}
