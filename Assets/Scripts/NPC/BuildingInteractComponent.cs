using System;
using UnityEngine;

public class BuildingInteractComponent : MonoBehaviour
{
    public Building InteractBuilding;

    public bool IsInteracting { get; private set; } = false;

    public event Action<Building> onSetedInteractBuilding;
    public event Action<Building> onRemovedInteractBuilding;
    public event Action<Building> onInteractionStarted;
    public event Action<Building> onInteractionStopped;

    public static event Action<BuildingInteractComponent> onInteractorSetedInteractBuilding;
    public static event Action<BuildingInteractComponent> onInteractorRemovedInteractBuilding;

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
            TryStopInteracting();
        }

        var lastInteractBuilding = InteractBuilding;
        InteractBuilding = null;

        onRemovedInteractBuilding?.Invoke(lastInteractBuilding);
        onInteractorRemovedInteractBuilding?.Invoke(this);
    }

    public void TryStartInteracting()
    {
        if (!ShouldStartInteracting()) return;

        StartInteracting();
    }

    public void TryStopInteracting()
    {
        if (!ShouldStopInteracting()) return;

        StopInteracting();
    }

    private void StartInteracting()
    {
        IsInteracting = true;
        onInteractionStarted?.Invoke(InteractBuilding);
    }

    private void StopInteracting()
    {
        IsInteracting = false;
        onInteractionStopped?.Invoke(InteractBuilding);
    }

    private bool ShouldStartInteracting()
    {
        if (IsInteracting) return false;

        return true;
    }

    private bool ShouldStopInteracting()
    {
        if (!IsInteracting) return false;

        return true;
    }
}