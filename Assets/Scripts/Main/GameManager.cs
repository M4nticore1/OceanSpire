using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ResourceStack
{
    public ItemData resource;
    public int amount;
}

public class GameManager : MonoBehaviour
{
    private  PlayerController playerController;
    public static CityManager cityManager;

    [Header("Buildings")]
    public List<Building> buildingPrefabs = new List<Building>();
    public List<Boat> boatPrefabs = new List<Boat>();
    public const float demolitionResourceRefundRate = 0.2f;

    // Items
    [Header("Items")]
    //public static List<ItemData> itemsData = new List<ItemData>();
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

    public static SaveData saveData = null;

    private void Start()
    {
        cityManager = FindAnyObjectByType<CityManager>();
        playerController = FindAnyObjectByType<PlayerController>();

        ItemDatabase.Load();

        saveData = SaveSystem.LoadData();
        cityManager.Load(saveData);
        //playerController.Load(saveData);

        ChangeWind();
        windDirection = newWindDirection;
    }

    private void Update()
    {
        //if (!playerController)
        //    playerController = FindAnyObjectByType<PlayerController>();

        //if (playerController.isInitialized)
        //    playerController.Tick();
        //else
        //    playerController.Load(saveData);

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

    public Building GetBuildingPrefabById(int buildingId)
    {
        for (int i = 0; i < buildingPrefabs.Count; i++)
        {
            if ((int)buildingPrefabs[i].BuildingData.BuildingIdValue == buildingId)
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
            if (buildingPrefabs[i].BuildingData.BuildingIdName == buildingIdName)
            {
                return buildingPrefabs[i];
            }
        }

        return null;
    }
}
