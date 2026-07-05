#if UNITY_EDITOR
using UnityEngine;

public class EditorConstructionTestController : MonoBehaviour
{
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private BuildingCostSystem buildingCostSystem;
    [SerializeField] private bool testMode = false;

    private void OnEnable()
    {
        ConstructionComponent.OnGlobalConstructionStarted += OnConstructionStarted;
    }

    private void OnDisable()
    {
        ConstructionComponent.OnGlobalConstructionStarted -= OnConstructionStarted;
    }

    private void Start()
    {
        UpdateMode();
    }

    private void Reset()
    {
        UpdateMode();
    }

    private void UpdateMode()
    {
        if (testMode) {
            FinishAllContructions();
            buildingCostSystem.gameObject.SetActive(false);
        }
        else {
            buildingCostSystem.gameObject.SetActive(true);
        }
    }

    private void FinishAllContructions()
    {
        foreach (var building in buildingsManager.GerGroundBuildings()) {
            building.ConstructionComponent.FinishConstruction();
        }

        foreach (var building in buildingsManager.GetTowerBuildings()) {
            building.ConstructionComponent.FinishConstruction();
        }
    }

    private void OnConstructionStarted(ConstructionComponent construction)
    {
        construction.FinishConstruction();
    }
}
#endif