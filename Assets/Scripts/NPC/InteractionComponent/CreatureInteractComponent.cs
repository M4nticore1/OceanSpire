using System;
using UnityEngine;

public class CreatureInteractComponent : MonoBehaviour
{
    [field: SerializeField] public Building InteractBuilding { get; private set; }
    [field: SerializeField] public bool IsInteracting { get; private set; } = false;

    public event Action<Building> OnInteractBuildingSeted;
    public event Action<Building> OnInteractBuildingRemoved;
    public event Action<Building> OnInteractionStarted;
    public event Action<Building> OnInteractionStopped;

    public static event Action<CreatureInteractComponent> OnInteractorInteractBuildingSeted;
    public static event Action<CreatureInteractComponent> OnInteractorInteractBuildirngRemoved;

    private void OnEnable()
    {
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    public void Init()
    {
        Init(InteractionComponentData.Default() ?? new InteractionComponentData());
    }

    public void Init(InteractionComponentData interactionData)
    {
        if (interactionData == null) {
            Debug.LogError("interactionData is not valid");
            Init();
            return;
        }

        var instanceId = interactionData.InteractBuildingInstanceId;
        if (instanceId == null) return;

        var instance = InstancesManager.Instance.GetInstance(instanceId.Value);

        if (instance) {
            var interactBuilding = instance.GetComponent<Building>();
            SetInteractBuilding(interactBuilding);
        }
    }

    public void SetInteractBuilding(Building building)
    {
        if (!building) {
            Debug.LogError($"[{nameof(CreatureInteractComponent)}] Building not found. Use RemoveInteractBuilding method instead of SetInteractBuilding.");
            return;
        }

        if (building == InteractBuilding) {
            Debug.LogError($"[{nameof(CreatureInteractComponent)}] Building {building} is already interact building");
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

    public void TryStopInteracting(Building building)
    {
        if (!ShouldStopInteracting(building)) return;

        StopInteracting(building);
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

    private void OnBuildingDemolished(Building building)
    {
        if (building != InteractBuilding) return;

        RemoveInteractBuilding();
    }

    private bool ShouldStartInteracting(Building building)
    {
        if (!building) return false;
        if (IsInteracting) return false;

        return true;
    }

    private bool ShouldStopInteracting(Building building)
    {
        if (!building) return false;
        if (!IsInteracting) return false;

        return true;
    }
}