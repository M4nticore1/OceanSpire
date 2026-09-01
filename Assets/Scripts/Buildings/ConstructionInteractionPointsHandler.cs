using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionInteractionPointsHandler : MonoBehaviour
{
    [SerializeField] private BuildingConstruction buildingConstruction;

    [SerializeField] private List<CreatureCityNavigator> interactors = new();
    private Dictionary<CreatureCityNavigator, BuildingAction> interactorsDict = new();

    private Coroutine AssignInteractorCoroutine;
    private Coroutine RemoveInteractorCoroutine;

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

    public void RunAssignInteractorEndOfFrame(CreatureCityNavigator navigator)
    {
        if (AssignInteractorCoroutine == null) {
            AssignInteractorCoroutine = StartCoroutine(AssignInteractorEndOfFrame(navigator));
        }
    }

    public void RunRemoveInteractorEndOfFrame(CreatureCityNavigator navigator)
    {
        if (RemoveInteractorCoroutine == null) {
            RemoveInteractorCoroutine = StartCoroutine(RemoveInteractorEndOfFrame(navigator));
        }
    }

    public void AssignInteractor(CreatureCityNavigator navigator)
    {
        if (navigator == null) return;
        if (interactorsDict.ContainsKey(navigator)) return;

        var index = GetFirstFreeIndex();
        if (index == null) return;

        if (RemoveInteractorCoroutine != null) {
            StopCoroutine(RemoveInteractorCoroutine);
        }

        interactors.Add(navigator);
        interactorsDict.Add(navigator, GetInteractPoint(index.Value));
    }

    public void RemoveInteractor(CreatureCityNavigator navigator)
    {
        if (navigator == null) return;

        interactors.Remove(navigator);
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

    private IEnumerator AssignInteractorEndOfFrame(CreatureCityNavigator navigator)
    {
        yield return new WaitForEndOfFrame();

        AssignInteractorCoroutine = null;
        AssignInteractor(navigator);
    }

    private IEnumerator RemoveInteractorEndOfFrame(CreatureCityNavigator navigator)
    {
        yield return new WaitForEndOfFrame();

        RemoveInteractorCoroutine = null;
        RemoveInteractor(navigator);
    }

    private int? GetFirstFreeIndex()
    {
        var actions = buildingConstruction.BuildingInteractions;
        if (actions == null) return null;
        if (actions.Length == 0) return null;

        for (int i = 0; i < actions.Length; i++) {
            if (IsIndexOccupied(i)) continue;

            return i;
        }

        return interactorsDict.Count % actions.Length;
    }

    private bool IsIndexOccupied(int index)
    {
        foreach (var action in interactorsDict.Values) {
            if (action == null) continue;
            if (action != GetInteractPoint(index)) continue;

            return true;
        }

        return false;
    }
}