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
    private CityManager cityManager = null;
    private LootManager lootManager = null;

    [Header("Content")]
    [SerializeField] private LootList lootList = null;
    [SerializeField] private BuildingPrefabsList buildingPrefabsList = null;
    [SerializeField] private BoatPrefabsList boatPrefabsList = null;

    public LootList LootList => lootList;
    public BuildingPrefabsList BuildingPrefabsList => buildingPrefabsList;
    public BoatPrefabsList BoatPrefabsList => boatPrefabsList;

    [Header("NPC")]
    public Resident residentPrefab = null;

    // Wind
    [HideInInspector] public Vector2 windDirection = Vector2.zero;
    [HideInInspector] public float windRotation = 0;
    private Vector2 newWindDirection = Vector2.zero;

    public const float windSpeed = 15.0f;
    private const float windChangingSpeed = 0.05f;
    private float windDirectionChangeRate = 300.0f;
    private float windDirectionChangeTime = 0.0f;

    // Other
    public const float autoSaveFrequency = 1;
    public const float triggerLootContainerRadius = 150f;
    public const float demolitionResourceRefundRate = 0.2f;

    public static SaveData saveData = null;

    private void Awake()
    {
        buildingPrefabsList.Initialize();
    }

    private void Start()
    {
        cityManager = FindAnyObjectByType<CityManager>();
        lootManager = FindAnyObjectByType<LootManager>();

        TimerManager.Initialize();
        //ItemDatabase.Load();

        ChangeWind();
        windDirection = newWindDirection;
        if (lootManager)
            lootManager.Initialize();
        else
            Debug.LogError("LootManager is NULL");

        saveData = SaveSystem.LoadData();
        cityManager.Load(saveData);

        //playerController.Load(saveData);


        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
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
        TimerManager.Tick();
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
}
