using System.Collections.Generic;
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

        var index = GetFirstFreeIndex();
        interactorsDict.Add(navigator, GetInteractPoint(index));
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
        if (actions == null || actions.Length <= 0) {
            Debug.LogError($"[{nameof(ConstructionInteractionPointsHandler)}] Interactions count is 0 at {name}!");
            return null;
        }

        index %= actions.Length;
        return actions[index];
    }

    public BuildingAction GetInteractPoint(CreatureCityNavigator navigator)
    {
        if (navigator == null) return null;
        if (!interactorsDict.ContainsKey(navigator)) return null;

        return interactorsDict[navigator];
    }

    private int GetFirstFreeIndex()
    {
        var index = 0;

        while (IsIndexOccupied(index)) {
            index++;
        }

        return index;
    }

    private bool IsIndexOccupied(int index)
    {
        foreach (var action in interactorsDict.Values) {
            if (action == GetInteractPoint(index)) {
                return true;
            }
        }

        return false;
    }
}