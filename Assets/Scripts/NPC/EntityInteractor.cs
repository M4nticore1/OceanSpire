using System;
using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
public class EntityInteractor : MonoBehaviour
{
    private EntityMovement movement = null;

    [SerializeField] private Building interactBuilding = null;
    public Building InteractBuilding => interactBuilding;

    public bool isInteracting { get; private set; } = false;
    public int interactorIndex { get; private set; } = 0;

    private double currentActionTime = 0.0f;
    private int currentActionIndex = 0;
    private const float takeItemDuration = 1.0f;

    public event Action<Building> onSetedInteractBuilding;
    public event Action<Building> onRemovedInteractBuilding;
    public event Action<Building> onStartedInteracting;
    public event Action<Building> onStoppedInteracting;

    private void Awake()
    {
        movement = GetComponent<EntityMovement>();
    }

    private void OnEnable()
    {
        EventBus.onCitizenWidgetClicked += OnWorkerWidgetClicked;
    }

    private void OnDisable()
    {
        EventBus.onCitizenWidgetClicked -= OnWorkerWidgetClicked;
    }

    private void Update()
    {
        if (isInteracting) {
            Interacting();
        }
    }

    // Events
    private void OnWorkerWidgetClicked(CitizenWidget widget)
    {
        Human resident = widget.citizen;
        if (resident != GetComponent<Human>()) return;

        Building selectedBuilding = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        if (interactBuilding) {
            if (selectedBuilding == interactBuilding) {
                RemoveInteractBuilding();
            }
            else {
                if (selectedBuilding.workers.Count < selectedBuilding.ConstructionLevelsData[selectedBuilding.LevelIndex].maxResidentsCount) {
                    RemoveInteractBuilding();
                    SetInteractBuilding(selectedBuilding);
                }
            }
        }
        else {
            if (selectedBuilding.workers.Count < selectedBuilding.ConstructionLevelsData[selectedBuilding.LevelIndex].maxResidentsCount) {
                SetInteractBuilding(selectedBuilding);
            }
        }
    }

    public void OnStoppedMoving()
    {
        StartInteracting();
    }

    public void SetInteractBuilding(Building building)
    {
        if (!building) {
            Debug.LogError("building is null.");
            return;
        }

        interactBuilding = building;
        AssignInteractorIndex();
        building.AddWorker(this);
        
        onSetedInteractBuilding?.Invoke(building);
        EventBus.InvokeSetedInteractBuilding();
    }

    private void RemoveInteractBuilding()
    {
        if (isInteracting) {
            StopInteracting();
        }

        Building lastBuilding = interactBuilding;
        interactBuilding = null;
        AssignInteractorIndex();
        lastBuilding.RemoveWorker(this);

        onRemovedInteractBuilding?.Invoke(lastBuilding);
        EventBus.InvokeRemovedInteractBuilding();
    }

    private void AssignInteractorIndex()
    {
        interactorIndex = interactBuilding ? interactBuilding.workers.Count : 0;
    }

    private void StartInteracting()
    {
        interactBuilding.AddCurrentWorker(this);
        isInteracting = true;
        //movement.MoveTo(InteractBuilding.GetInteractionTransform().position);
        onStartedInteracting?.Invoke(interactBuilding);
    }

    private void StopInteracting()
    {
        if (!isInteracting) return;

        interactBuilding.RemoveCurrentWorker(this);
        isInteracting = false;
        onStoppedInteracting?.Invoke(interactBuilding);
    }

    private void Interacting()
    {

    }
}