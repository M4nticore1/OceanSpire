using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioStationsManager : MonoBehaviour
{
    public static RadioStationsManager Instance { get; private set; }

    private List<RadioStationModule> radioStations = new();
    public IReadOnlyList<RadioStationModule> RadioStations => radioStations;

    [field: SerializeField] public float currentWandererCooldownSpeedBonus { get; private set; } = 1f;

    private Coroutine UpdateSpeedBonusCoroutine;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Debug.LogError($"[{nameof(RadioStationsManager)}] There's another Radio Stations Manager in the scene!");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        Building.OnBuildingConstructionStarted += HandleBuildingConstructionStarted;
        Building.OnBuildingConstructionFinished += HandleBuildingConstructionFinished;
        Building.OnBuildingUpgradeFinished += HandleBuildingUpgradeFinished;

        BuildingModule.OnModuleWorkingStarted += HandleBuildingWorkingStarted;
        BuildingModule.OnModuleWorkingStopped += HandleBuildingWorkingStopped;
    }

    private void OnDisable()
    {
        Building.OnBuildingConstructionStarted -= HandleBuildingConstructionStarted;
        Building.OnBuildingConstructionFinished -= HandleBuildingConstructionFinished;
        Building.OnBuildingUpgradeFinished -= HandleBuildingUpgradeFinished;

        BuildingModule.OnModuleWorkingStarted -= HandleBuildingWorkingStarted;
        BuildingModule.OnModuleWorkingStopped -= HandleBuildingWorkingStopped;
    }

    public void RegisterRadioStation(RadioStationModule radioStation)
    {
        if (radioStation == null) return;
        if (radioStations.Contains(radioStation)) return;

        radioStations.Add(radioStation);
        RunTryUpdateSpeedBonusCoroutine(radioStation.gameObject);
    }

    public void UnregisterRadioStation(RadioStationModule radioStation)
    {
        if (radioStation == null) return;

        radioStations.Remove(radioStation);
        RunTryUpdateSpeedBonusCoroutine(radioStation.gameObject);
    }

    private void RunTryUpdateSpeedBonusCoroutine(GameObject go)
    {
        if (this == null) return;
        if (go == null) return;

        if (UpdateSpeedBonusCoroutine == null) {
            UpdateSpeedBonusCoroutine = StartCoroutine(TryUpdateSpeedBonusCoroutine(go));
        }
    }

    private void UpdateSpeedBonus()
    {
        var bonus = 1f;
        foreach (var radioStation in radioStations) {
            if (radioStation == null) continue;
            if (!radioStation.IsWorking) continue;

            var ownedBuilding = radioStation.OwnedBuilding;
            if (ownedBuilding == null) {
                Debug.LogError($"[{nameof(RadioStationsManager)}] Owned Building is not valid at {radioStation}!");
                continue;
            }

            var constructionComponent = ownedBuilding.ConstructionComponent;
            if (constructionComponent == null) {
                Debug.LogError($"[{nameof(RadioStationsManager)}] Construction Component is not valid at {ownedBuilding}!");
                continue;
            }

            if (constructionComponent.IsUnderConstruction) continue;

            var levelData = radioStation.RadioStationLevelData;
            if (levelData == null) {
                Debug.LogError($"[{nameof(RadioStationsManager)}] Radio Station Level Data is not valid at {ownedBuilding}!");
                continue;
            }

            bonus += levelData.WandererCooldownSpeedBonus;
        }

        currentWandererCooldownSpeedBonus = Mathf.Max(1, bonus);
    }

    private void HandleBuildingConstructionStarted(Building building)
    {
        RunTryUpdateSpeedBonusCoroutine(building.gameObject);
    }

    private void HandleBuildingConstructionFinished(Building building)
    {
        RunTryUpdateSpeedBonusCoroutine(building.gameObject);
    }

    private void HandleBuildingUpgradeFinished(Building building)
    {
        RunTryUpdateSpeedBonusCoroutine(building.gameObject);
    }

    private void HandleBuildingWorkingStarted(BuildingModule module)
    {
        RunTryUpdateSpeedBonusCoroutine(module.gameObject);
    }

    private void HandleBuildingWorkingStopped(BuildingModule module)
    {
        RunTryUpdateSpeedBonusCoroutine(module.gameObject);
    }

    private bool TryUpdateSpeedBonus(GameObject go)
    {
        if (!ShouldUpdateSpeedBonus(go)) return false;

        UpdateSpeedBonus();
        return true;
    }

    private bool ShouldUpdateSpeedBonus(GameObject go)
    {
        if (go == null) return false;

        var radioStation = go.GetComponent<RadioStationModule>();
        if (radioStation == null) return false;

        return true;
    }

    private IEnumerator TryUpdateSpeedBonusCoroutine(GameObject go)
    {
        if (go == null) yield break;

        yield return new WaitForEndOfFrame();

        TryUpdateSpeedBonus(go);
        UpdateSpeedBonusCoroutine = null;
    }
}