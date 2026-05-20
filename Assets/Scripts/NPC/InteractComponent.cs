using System;
using UnityEngine;

public class InteractComponent : MonoBehaviour
{
    public Building InteractBuilding;

    public bool IsInteracting { get; private set; } = false;
    public int workerIndex { get; private set; } = 0;
    public int raiderIndex { get; private set; } = 0;

    public event Action<Building> onSetedInteractBuilding;
    public event Action<Building> onRemovedInteractBuilding;
    public event Action<Building> onInteractionStarted;
    public event Action<Building> onInteractionStopped;

    public static event Action<InteractComponent> onInteractorSetedInteractBuilding;
    public static event Action<InteractComponent> onInteractorRemovedInteractBuilding;

    public void SetInteractBuilding(Building building)
    {
        if (building == InteractBuilding) {
            Debug.Log($"Building {building} is already interact building");
            return;
        }

        if (!building) {
            RemoveInteractBuilding();
            return;
        }

        InteractBuilding = building;

        onSetedInteractBuilding?.Invoke(building);
        onInteractorSetedInteractBuilding?.Invoke(this);
    }

    public void TryRemoveInteractBuilding()
    {
        if (!InteractBuilding) return;

        RemoveInteractBuilding();
    }

    public void RemoveInteractBuilding()
    {
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
        onInteractionStarted?.Invoke(InteractBuilding);
    }

    public void StopInteracting()
    {
        if (!IsInteracting) return;
        
        IsInteracting = false;
        onInteractionStopped?.Invoke(InteractBuilding);
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