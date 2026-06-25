using System;
using UnityEngine;

public class CreatureInteractComponent : MonoBehaviour
{
    [field: SerializeField] public Building InteractBuilding { get; private set; }
    public bool IsInteracting { get; private set; } = false;

    public event Action<Building> OnInteractBuildingSeted;
    public event Action<Building> OnInteractBuildingRemoved;
    public event Action<Building> OnInteractionStarted;
    public event Action<Building> OnInteractionStopped;

    public static event Action<CreatureInteractComponent> OnInteractorInteractBuildingSeted;
    public static event Action<CreatureInteractComponent> OnInteractorInteractBuildirngRemoved;

    public void Init(InteractionComponentData interactionData)
    {
        if (interactionData == null) {
            Debug.LogError("interactionData is not valid", this);
            return;
        }

        var instanceId = interactionData.InteractBuildingInstanceId;
        if (instanceId == null) return;

        var instance = InstancesManager.Instance.GetInstance(instanceId.Value);
        var interactBuilding = instance?.GetComponent<Building>();

        SetInteractBuilding(interactBuilding);
    }

    public void SetInteractBuilding(Building building)
    {
        if (!building) {
            Debug.Log("Building not found. Use RemoveInteractBuilding method instead of SetInteractBuilding.");
            return;
        }

        if (building == InteractBuilding) {
            Debug.Log($"Building {building} is already interact building");
            return;
        }

        InteractBuilding = building;

        OnInteractBuildingSeted?.Invoke(building);
        OnInteractorInteractBuildingSeted?.Invoke(this);
    }

    public void RemoveInteractBuilding()
    {
        if (!InteractBuilding) return;

        var lastInteractBuilding = InteractBuilding;
        InteractBuilding = null;

        OnInteractBuildingRemoved?.Invoke(lastInteractBuilding);
        OnInteractorInteractBuildirngRemoved?.Invoke(this);
    }

    public void TryStartInteracting()
    {
        if (!ShouldStartInteracting(InteractBuilding)) return;

        StartInteracting();
    }

    public void TryStopInteracting()
    {
        if (!ShouldStopInteracting(InteractBuilding)) return;

        StopInteracting(InteractBuilding);
    }

    private void StartInteracting()
    {
        IsInteracting = true;
        OnInteractionStarted?.Invoke(InteractBuilding);
    }

    private void StopInteracting(Building building)
    {
        IsInteracting = false;
        OnInteractionStopped?.Invoke(building);
    }

    private bool ShouldStartInteracting(Building interactBuilding)
    {
        if (!interactBuilding) return false;
        if (IsInteracting) return false;

        return true;
    }

    private bool ShouldStopInteracting(Building interactBuilding)
    {
        if (!interactBuilding) return false;
        if (!IsInteracting) return false;

        return true;
    }
}