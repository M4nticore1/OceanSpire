using System;
using UnityEngine;

public class InteractComponent : MonoBehaviour
{
    public Building InteractBuilding { get; private set; }

    public bool IsInteracting { get; private set; } = false;
    public int workerIndex { get; private set; } = 0;
    public int raiderIndex { get; private set; } = 0;

    public event Action<Building> onSetedInteractBuilding;
    public event Action<Building> onRemovedInteractBuilding;
    public event Action onInteractionStarted;
    public event Action onInteractionStopped;

    public static event Action<InteractComponent> onInteractorSetedInteractBuilding;
    public static event Action<InteractComponent> onInteractorRemovedInteractBuilding;

    public void SetInteractBuilding(Building building)
    {
        InteractBuilding = building;
        onSetedInteractBuilding?.Invoke(building);
        onInteractorSetedInteractBuilding?.Invoke(this);
    }

    public void RemoveInteractBuilding()
    {
        if (!InteractBuilding) return;

        if (IsInteracting) {
            StopInteracting();
        }

        Building lastInteractBuilding = InteractBuilding;
        InteractBuilding = null;

        onRemovedInteractBuilding?.Invoke(lastInteractBuilding);
        onInteractorRemovedInteractBuilding?.Invoke(this);
    }

    public void StartInteracting()
    {
        IsInteracting = true;
        onInteractionStarted?.Invoke();
    }

    public void StopInteracting()
    {
        if (!IsInteracting) return;
        
        IsInteracting = false;
        onInteractionStopped?.Invoke();
    }

    public void AssignWorkerIndex()
    {
        workerIndex = InteractBuilding ? InteractBuilding.WorkComponent.Workers.Count : -1;
    }

    public void AssignRaiderIndex()
    {
        raiderIndex = InteractBuilding ? InteractBuilding.WorkComponent.Workers.Count : -1;
    }
}