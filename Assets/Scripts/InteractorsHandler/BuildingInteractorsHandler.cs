using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingInteractorsHandler : MonoBehaviour
{
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

    // Workers
    public void AddInteractor(Human interactor)
    {
        if (interactors.Contains(interactor)) return;

        interactors.Add(interactor);
        OnInteractorAdded?.Invoke(interactor);
        OnComponentInteractorAdded?.Invoke(this, interactor);
    }

    public void RemoveInteractor(Human interactor)
    {
        if (!interactors.Contains(interactor)) return;

        interactors.Remove(interactor);
        OnInteractorRemoved?.Invoke(interactor);
        OnComponentInteractorRemoved?.Invoke(this, interactor);
    }

    public void AddCurrentInteractor(Human interactor)
    {
        if (currentInteractors.Contains(interactor)) return;

        currentInteractors.Add(interactor);
        OnCurrentInteractorAdded?.Invoke(interactor);
        OnComponentCurrentInteractorAdded?.Invoke(this, interactor);
    }

    public void RemoveCurrentInteractor(Human interactor)
    {
        if (!currentInteractors.Contains(interactor)) return;

        currentInteractors.Remove(interactor);
        OnCurrentInteractorRemoved?.Invoke(interactor);
        OnComponentCurrentInteractorRemoved?.Invoke(this, interactor);
    }

    public int? TryGetInteractorIndex(Human interactor)
    {
        if (!interactor) {
            Debug.Log($"[{nameof(BuildingInteractorsHandler)}] Interactor not found at {name}");
            return null;
        }

        if (!interactors.Contains(interactor)) {
            Debug.Log($"[{nameof(BuildingInteractorsHandler)}] Interactor not found at Workers");
            return null;
        }

        return interactors.IndexOf(interactor);
    }
}