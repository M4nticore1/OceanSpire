using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance { get; private set; } = null;

    public Building buildingToPlace { get; private set; } = null;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.onStartedPlacingBuilding += OnSelectedBuildingToPlace;
        Building.onBuildingInited += OnBuildingFinishedPlacing;
    }

    private void OnDisable()
    {
        EventBus.onStartedPlacingBuilding -= OnSelectedBuildingToPlace;
        Building.onBuildingInited -= OnBuildingFinishedPlacing;
    }

    private void OnSelectedBuildingToPlace(Building building)
    {
        buildingToPlace = building;
    }

    private void OnBuildingFinishedPlacing(Building building)
    {
        buildingToPlace = null;
    }
}
