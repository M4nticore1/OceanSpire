using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstructionInteractionPointsHandler : MonoBehaviour
{
    [SerializeField] private BuildingConstruction buildingConstruction;

    private Dictionary<CreatureCityNavigator, BuildingAction> interactorsDict = new();
    public IReadOnlyDictionary<CreatureCityNavigator, BuildingAction> InteractorsDict => interactorsDict;

    public void Init()
    {
        if (buildingConstruction == null) {
            Debug.LogError($"[{nameof(ConstructionInteractionPointsHandler)}] Building Construction is not valid at {this}!");
            return;
        }

        var ownedBuilding = buildingConstruction.OwnedBuilding;
        if (ownedBuilding == null) return;

        UpdateInteractionTransforms(ownedBuilding.CitizensHandler);
        UpdateInteractionTransforms(ownedBuilding.RaidersHandler);
    }

    public void AssignInteractor(CreatureCityNavigator navigator)
    {
        if (navigator == null) return;
        if (interactorsDict.ContainsKey(navigator)) return;

        interactorsDict.Add(navigator, GetInteractPoint(interactorsDict.Count));
    }

    public void RemoveInteractor(CreatureCityNavigator navigator)
    {
        if (navigator == null) return;

        interactorsDict.Remove(navigator);
    }

    public void UpdateInteractionTransforms(BuildingInteractorsHandler interactorsHandler)
    {
        for (int i = 0; i < interactorsHandler.Interactors.Count; i++) {
            var interactor = interactorsHandler.Interactors[i];
            if (interactor == null) continue;

            var navigator = interactor.CityNavigator;
            if (navigator == null) continue;

            AssignInteractor(navigator);
        }
    }

    public BuildingAction GetInteractPoint(int index)
    {
        if (index < 0) {
            Debug.LogError($"[{nameof(ConstructionInteractionPointsHandler)}] Index is negative ({index})!");
            return null;
        }

        if (buildingConstruction == null) {
            Debug.LogError($"[{nameof(ConstructionInteractionPointsHandler)}] Spawned Construction is not valid at {this}!");
            return null;
        }

        var actions = buildingConstruction.BuildingInteractions;
        if (actions.Length <= 0) {
            Debug.LogError($"[{nameof(ConstructionInteractionPointsHandler)}] Intreactions count is 0 at {name}!");
            return null;
        }

        index %= actions.Length;
        return actions[index];
    }

    public BuildingAction GetInteractPoint(CreatureCityNavigator navigator)
    {
        if (!interactorsDict.ContainsKey(navigator)) {
            //Debug.LogError($"[{nameof(ConstructionInteractionPointsHandler)}] Intreactions doesn't contain {navigator} at {this}!");
            return null;
        }

        return interactorsDict[navigator];
    }
}