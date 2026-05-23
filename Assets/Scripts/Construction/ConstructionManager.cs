using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance { get; private set; } = null;

    public Building BuildingToPlace { get; private set; } = null;

    private void Awake()
    {
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

    private void OnSelectedBuildingToPlace(Building building)
    {
        BuildingToPlace = building;
    }

    private void OnBuildingFinishedPlacing(Building building)
    {
        BuildingToPlace = null;
    }
}
