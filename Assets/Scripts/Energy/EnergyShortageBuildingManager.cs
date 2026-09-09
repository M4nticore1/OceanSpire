using UnityEngine;

public class EnergyShortageBuildingManager : MonoBehaviour
{
    [SerializeField] private EnergyShortageManager energyShortageManager;
    [SerializeField] private BuildingsManager buildingsManager;

    private void OnEnable()
    {
        if (energyShortageManager != null) {
            energyShortageManager.OnEnergyShortageStarted += HandleEnergyShortageStarted;
            energyShortageManager.OnEnergyShortageEnded += HandleEnergyShortageEnded;
        }
        else {
            Debug.LogError($"[{nameof(EnergyShortageBuildingManager)}] Energy Shortage Manager is not valid!");
        }

        Building.OnBuildingInited += HandleBuildingInited;
    }

    private void OnDisable()
    {
        if (energyShortageManager != null) {
            energyShortageManager.OnEnergyShortageStarted -= HandleEnergyShortageStarted;
            energyShortageManager.OnEnergyShortageEnded -= HandleEnergyShortageEnded;
        }

        Building.OnBuildingInited -= HandleBuildingInited;
    }

    private void SetBuildingsModulesUnderEnergyShortage(bool underShortage)
    {
        foreach (var building in buildingsManager.GetTowerBuildings()) {
            SetBuildingModulesUnderEnergyShortage(building, underShortage);
        }
    }

    private void SetBuildingModulesUnderEnergyShortage(Building building, bool underShortage)
    {
        if (building == null) return;

        var modules = building.BuildingModules;
        foreach (var module in modules) {
            if (module == null) continue;

            module.SetUnderEnergyShortage(underShortage);
        }
    }

    private void HandleEnergyShortageStarted()
    {
        SetBuildingsModulesUnderEnergyShortage(true);
    }

    private void HandleEnergyShortageEnded()
    {
        SetBuildingsModulesUnderEnergyShortage(false);
    }

    private void HandleBuildingInited(Building building)
    {
        if (energyShortageManager.IsUnderEnergyShortage) {
            SetBuildingModulesUnderEnergyShortage(building, true);
        }
    }
}