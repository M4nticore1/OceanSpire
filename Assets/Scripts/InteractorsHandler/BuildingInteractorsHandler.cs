using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingInteractorsHandler : MonoBehaviour
{
    [field: SerializeField] private List<Human> entered = new();
    public IReadOnlyList<Human> EnteredInteractors => entered.AsReadOnly();

    [field: SerializeField] private List<Human> interactors = new();
    public IReadOnlyList<Human> Interactors => interactors.AsReadOnly();

    [field: SerializeField] private List<Human> currentInteractors = new();
    public IReadOnlyList<Human> CurrentInteractors => currentInteractors.AsReadOnly();

    public event Action<Human> OnInteractorAdded;
    public event Action<Human> OnInteractorRemoved;

    public event Action<Human> OnCurrentInteractorAdded;
    public event Action<Human> OnCurrentInteractorRemoved;

    public static event Action<BuildingInteractorsHandler, Human> OnComponentInteractorAdded;
    public static event Action<BuildingInteractorsHandler, Human> OnComponentInteractorRemoved;

    public static event Action<BuildingInteractorsHandler, Human> OnComponentCurrentInteractorAdded;
    public static event Action<BuildingInteractorsHandler, Human> OnComponentCurrentInteractorRemoved;

    // Entered
    public void AddEnteredInteractor(Human interactor)
    {
        if (!TryAddInteractor(interactor, ref entered)) return;

        OnInteractorAdded?.Invoke(interactor);
        OnComponentInteractorAdded?.Invoke(this, interactor);
    }

    public void RemoveEnteredInteractor(Human interactor)
    {
        if (!TryRemoveInteractor(interactor, ref entered)) return;

        OnInteractorRemoved?.Invoke(interactor);
        OnComponentInteractorRemoved?.Invoke(this, interactor);
    }

    // Interactors
    public void AddInteractor(Human interactor)
    {
        if (!TryAddInteractor(interactor, ref interactors)) return;

        OnInteractorAdded?.Invoke(interactor);
        OnComponentInteractorAdded?.Invoke(this, interactor);
    }

    public void RemoveInteractor(Human interactor)
    {
        if (!TryRemoveInteractor(interactor, ref interactors)) return;

        OnInteractorRemoved?.Invoke(interactor);
        OnComponentInteractorRemoved?.Invoke(this, interactor);
    }

    public void AddCurrentInteractor(Human interactor)
    {
        if (!TryAddInteractor(interactor, ref currentInteractors)) return;

        OnCurrentInteractorAdded?.Invoke(interactor);
        OnComponentCurrentInteractorAdded?.Invoke(this, interactor);
    }

    public void RemoveCurrentInteractor(Human interactor)
    {
        if (!TryRemoveInteractor(interactor, ref currentInteractors)) return;

        OnCurrentInteractorRemoved?.Invoke(interactor);
        OnComponentCurrentInteractorRemoved?.Invoke(this, interactor);
    }

    public int? TryGetInteractorIndex(Human interactor)
    {
        if (interactor == null) {
            Debug.Log($"[{nameof(BuildingInteractorsHandler)}] Interactor not found at {name}");
            return null;
        }

        var index = interactors.IndexOf(interactor);

        return index >= 0 ? index : null;
    }

    private bool TryAddInteractor(Human interactor, ref List<Human> list)
    {
        if (interactor == null) return false;
        if (list == null) return false;
        if (list.Contains(interactor)) return false;

        list.Add(interactor);
        return true;
    }

    private bool TryRemoveInteractor(Human interactor, ref List<Human> list)
    {
        if (interactor == null) return false;
        if (list == null) return false;

        return list.Remove(interactor);
    }
}