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
        EventBus.onBuildingStartedPlacing += OnBuildingStartedPlacing;
        EventBus.onBuildingFinishedPlacing += OnBuildingFinishedPlacing;
    }

    private void OnDisable()
    {
        EventBus.onBuildingStartedPlacing -= OnBuildingStartedPlacing;
        EventBus.onBuildingFinishedPlacing -= OnBuildingFinishedPlacing;
    }

    private void OnBuildingStartedPlacing(Building building)
    {
        buildingToPlace = building;
    }

    private void OnBuildingFinishedPlacing(Building building)
    {
        buildingToPlace = null;
    }
}
