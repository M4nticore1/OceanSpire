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

    [HideInInspector] public float windSpeed = 0.0f;
    private float newWindSpeed = 0.0f;
    private const float windMinSpeed = 4.0f;
    private const float windMaxSppeed = 10.0f;

    private float windSpeedChangingSpeed = 0.0f;
    private const float windMinSpeedChangingSpeed = 0.05f;
    private const float windMaxSpeedChangingSpeed = 0.1f;

    private float windDirectionChangeRate = 0.0f;
    private const float windDirectionChanceMinRate = 120.0f;
    private const float windDirectionChanceMaxRate = 300.0f;
    private float windDirectionChangeTime = 0.0f;

    private float windDirectionChangeSpeed = 0.0f;
    private const float windDirectionMinChangeSpeed = 0.04f;
    private const float windDirectionMaxChangeSpeed = 0.05f;

    public const float autoSaveFrequency = 1;

    public bool hasSavedData = false;

    private void Awake()
    {
        cityManager = FindAnyObjectByType<CityManager>();
    }

    private void Start()
    {
        ChangeWind();
        windSpeed = newWindSpeed;
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

        windDirection = math.lerp(windDirection, newWindDirection, windDirectionChangeSpeed * Time.deltaTime);
        windSpeed = math.lerp(windSpeed, newWindSpeed, windSpeedChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        float xAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        float yAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        newWindDirection = new Vector2(xAxis, yAxis).normalized;

        windDirectionChangeRate = UnityEngine.Random.Range(windDirectionChanceMinRate, windDirectionChanceMaxRate);
        windDirectionChangeSpeed = UnityEngine.Random.Range(windDirectionMinChangeSpeed, windDirectionMaxChangeSpeed);
        newWindSpeed = UnityEngine.Random.Range(windMinSpeed, windMaxSppeed);
        windSpeedChangingSpeed = UnityEngine.Random.Range(windMinSpeedChangingSpeed, windMaxSpeedChangingSpeed);

        windDirectionChangeTime = 0;
    }

    public int GetItemIndexByIdName(string idName)
    {
        int id = 0;

        for (int i = 0; i < itemsData.Count; i++)
        {
            if (itemsData[i].itemIdName == idName)
            {
                id = i;
                break;
            }
        }

        return id;
    }

    public int GetItemIndexById(int id)
    {
        int currentId = 0;

        for (int i = 0; i < itemsData.Count; i++)
        {
            if ((int)itemsData[i].itemId == id)
            {
                currentId = i;
                break;
            }
        }

        return currentId;
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
