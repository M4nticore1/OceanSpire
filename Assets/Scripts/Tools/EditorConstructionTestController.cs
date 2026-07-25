using UnityEngine;

public class EditorConstructionTestController : MonoBehaviour
{
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private BuildingCostSystem buildingCostSystem;
    [SerializeField] private bool testMode = false;

    private void OnEnable()
    {
#if UNITY_EDITOR
        ConstructionComponent.OnGlobalConstructionStarted += OnConstructionStarted;
        FinishAllContructions();
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        ConstructionComponent.OnGlobalConstructionStarted -= OnConstructionStarted;
#endif
    }

    private void Start()
    {
#if UNITY_EDITOR
        UpdateMode();
#endif
    }

    private void Reset()
    {
#if UNITY_EDITOR
        UpdateMode();
#endif
    }

    private void UpdateMode()
    {
#if UNITY_EDITOR
        if (testMode) {
            if (buildingCostSystem) {
                buildingCostSystem.gameObject.SetActive(false);
            }

            FinishAllContructions();
        }
        else {
            if (buildingCostSystem) {
                buildingCostSystem.gameObject.SetActive(true);
            }
        }
#endif
    }

    private void FinishAllContructions()
    {
#if UNITY_EDITOR
        if (!buildingsManager) return;

        foreach (var building in buildingsManager.GerGroundBuildings()) {
            building.ConstructionComponent.FinishConstruction();
        }

        foreach (var building in buildingsManager.GetTowerBuildings()) {
            building.ConstructionComponent.FinishConstruction();
        }
#endif
    }

    private void OnConstructionStarted(ConstructionComponent construction)
    {
#if UNITY_EDITOR
        construction.FinishConstruction();
#endif
    }
}