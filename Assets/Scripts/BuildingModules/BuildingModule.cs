using System;
using UnityEngine;

public abstract class BuildingModule : MonoBehaviour
{
    private Building ownedBuilding = null;
    public Building OwnedBuilding => ownedBuilding ? ownedBuilding : GetComponent<Building>();
    public TowerBuilding OwnedTowerBuilding => OwnedBuilding as TowerBuilding;

    [SerializeField] protected BuildingModuleLevelData[] levelsData = { };
    public BuildingModuleLevelData[] LevelsData => levelsData;
    public BuildingModuleLevelData LevelData
    {
        get
        {
            int level = ownedBuilding.LevelComponent.Level - 1;

            if (level < LevelsData.Length) {
                return LevelsData[level];
            }
            else {
                Debug.LogError(ownedBuilding.BuildingData.BuildingId.ToString() + $" has no level data by index {level}");
                return null;
            }
        }
    }
    protected BuildingConstruction BuildingConstruction => ownedBuilding.SpawnedConstruction;

    protected bool IsInited { get; private set; } = false;
    private bool isSubscribed = false;
    public bool IsWorking { get; private set; } = false;

    public event Action onWorkStarted;
    public event Action onWorkStopped;

    protected virtual void Awake()
    {
        ownedBuilding = GetComponent<Building>();
    }

    protected virtual void OnEnable()
    {
        TrySubscribe();
    }

    protected virtual void OnDisable()
    {
        TryUnsubscribe();
    }

    protected virtual void Subscribe()
    {
        ownedBuilding.OnInited += OnInit;
    }

    protected virtual void Unsubscribe()
    {
        ownedBuilding.OnInited -= OnInit;
    }

    protected virtual void OnInit()
    {
        IsInited = true;
    }

    protected void StartWorking()
    {
        if (IsWorking) return;

        IsWorking = true;
        onWorkStarted?.Invoke();
    }

    protected void StopWorking()
    {
        if (!IsWorking) return;

        IsWorking = false;
        onWorkStopped?.Invoke();
    }

    protected void SetFlickingPower(float multiplier)
    {
        BuildingConstruction.SetFlickingPower(multiplier);
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        Subscribe();

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        Unsubscribe();

        isSubscribed = false;
    }
}