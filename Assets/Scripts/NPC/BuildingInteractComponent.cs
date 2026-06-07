using System;
using UnityEngine;

public class BuildingInteractComponent : MonoBehaviour
{
    public Building InteractBuilding;

    public bool IsInteracting { get; private set; } = false;

    public event Action<Building> OnInteractBuildingSeted;
    public event Action<Building> OnInteractBuildingRemoved;
    public event Action<Building> OnInteractionStarted;
    public event Action<Building> OnInteractionStopped;

    public static event Action<BuildingInteractComponent> OnInteractorInteractBuildingSeted;
    public static event Action<BuildingInteractComponent> OnInteractorInteractBuildirngRemoved;

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

        OnInteractBuildingSeted?.Invoke(building);
        OnInteractorInteractBuildingSeted?.Invoke(this);
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

        OnInteractBuildingRemoved?.Invoke(lastInteractBuilding);
        OnInteractorInteractBuildirngRemoved?.Invoke(this);
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
        OnInteractionStarted?.Invoke(InteractBuilding);
    }

    private void StopInteracting()
    {
        IsInteracting = false;
        OnInteractionStopped?.Invoke(InteractBuilding);
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