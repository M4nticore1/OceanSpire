using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ResourceStack
{
    public ItemData resource;
    public int amount;
}

enum Direction
{
    Forward,
    Back
}

[Serializable]
public class BuildingPath
{
    public List<Building> paths = new List<Building>();
}

[Serializable]
public class CityData
{
    public string cityName = "";
    public int floorsCount = 0;
}

public class CityManager : MonoBehaviour
{
    public static CityManager Instance { get; private set; } = null;

    [SerializeField] private PlayerController playerController = null;

    // Buildings
    [Header("Buildings")]
    [SerializeField] private NavMeshSurface towerNavMeshSurface = null;

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;
    public bool isNavMeshBuilt { get; private set; } = false;

    // Other
    public const float autoSaveFrequency = 1;
    public const float triggerLootContainerRadius = 150f;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        WorldData saveData = WorldSaveManager.Instance.currentSaveWorldData;

        bakeNavMeshCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());
    }

    private void Update()
    {
        TimerManager.Tick();
    }

    private IEnumerator BakeNavMeshSurfaceCoroutine()
    {
        if (bakeNavMeshCoroutine != null) yield break;

        yield return new WaitForEndOfFrame();
        towerNavMeshSurface.BuildNavMesh();
        bakeNavMeshCoroutine = null;
        EventBus.InvokNavMeshBaked();
    }

    private IEnumerator AutosaveCoroutine()
    {
        while (true) {
            yield return new WaitForSeconds(autoSaveFrequency);
            WorldSaveSystem.SaveData(playerController);
        }
    }
}
