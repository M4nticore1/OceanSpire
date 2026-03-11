using System;
using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
public class EntityInteractor : MonoBehaviour
{
    [SerializeField] private Building interactBuilding = null;
    public Building InteractBuilding => interactBuilding;

    public bool isInteracting { get; private set; } = false;
    public int interactorIndex { get; private set; } = 0;

    public event Action<Building> onSetedInteractBuilding;
    public event Action<Building> onRemovedInteractBuilding;
    public event Action<Building> onStartedInteracting;
    public event Action<Building> onStoppedInteracting;

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

    public void SetInteractBuilding(Building building)
    {
        if (!building) {
            Debug.LogError("building is null.");
            return;
        }

        if (interactBuilding) {
            interactBuilding.RemoveWorker(this);
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

    public void HandleStoppedMoving()
    {
        if (!interactBuilding) return;

        StartInteracting();
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

    private void AssignInteractorIndex()
    {
        interactorIndex = interactBuilding ? interactBuilding.workers.Count : -1;
    }

    private void StartInteracting()
    {
        interactBuilding.AddCurrentWorker(this);
        isInteracting = true;
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