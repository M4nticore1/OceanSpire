using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance { get; private set; } = null;

    private readonly HashSet<ConstructionComponent> constructions = new();

    public Building BuildingToPlace { get; private set; } = null;

    private void Awake()
    {
        if (Instance) {
            Debug.LogError($"[{nameof(ConstructionManager)}] Another instance already exists in the scene! Destroying this.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.OnConstructionStarted += OnSelectedBuildingToPlace;
        Building.OnBuildingInited += OnBuildingFinishedPlacing;
    }

    private void OnDisable()
    {
        EventBus.OnConstructionStarted -= OnSelectedBuildingToPlace;
        Building.OnBuildingInited -= OnBuildingFinishedPlacing;
    }

    private void Update()
    {
        foreach (var construction in constructions) {
            construction.Tick();
        }
    }

    public void Register(ConstructionComponent constructionComponent)
    {
        if (!constructionComponent) return;

        constructions.Add(constructionComponent);
    }

    public void Unregister(ConstructionComponent constructionComponent)
    {
        if (!constructionComponent) return;

        constructions.Remove(constructionComponent);
    }

    private void OnSelectedBuildingToPlace(Building building)
    {
        BuildingToPlace = building;
    }

    private void OnBuildingFinishedPlacing(Building building)
    {
        BuildingToPlace = null;
    }
}
