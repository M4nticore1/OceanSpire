using System;
using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
public class EntityInteractor : MonoBehaviour
{
    public Building interactBuilding { get; private set; }

    public bool isInteracting { get; private set; } = false;
    public int workerIndex { get; private set; } = 0;
    public int raiderIndex { get; private set; } = 0;

    public event Action<Building> onSetedInteractBuilding;
    public event Action<Building> onRemovedInteractBuilding;
    public event Action<Building> onStartedInteracting;
    public event Action<Building> onStoppedInteracting;

    private void Update()
    {
        if (isInteracting) {
            Interacting();
        }
    }

    public void SetInteractBuilding(Building building)
    {
        interactBuilding = building;
    }

    public void RemoveInteractBuilding()
    {
        if (isInteracting) {
            StopInteracting();
        }

        interactBuilding = null;
    }

    public void StartInteracting()
    {
        interactBuilding.AddCurrentWorker(this);
        isInteracting = true;
        onStartedInteracting?.Invoke(interactBuilding);
    }

    // Events
    private void OnWorkerWidgetClicked(CitizenWidget widget)
    {
        Human resident = widget.human;
        if (resident != GetComponent<Human>()) return;

        Building selectedBuilding = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        if (interactBuilding) {
            if (selectedBuilding == interactBuilding) {
                RemoveInteractBuilding();
            }
            else {
                if (selectedBuilding.workers.Count < selectedBuilding.LevelsData[selectedBuilding.LevelIndex].maxResidentsCount) {
                    RemoveInteractBuilding();
                    SetInteractBuilding(selectedBuilding);
                }
            }
        }
        else {
            if (selectedBuilding.workers.Count < selectedBuilding.LevelsData[selectedBuilding.LevelIndex].maxResidentsCount) {
                SetInteractBuilding(selectedBuilding);
            }
        }
    }

    public void AssignWorkerIndex()
    {
        workerIndex = interactBuilding ? interactBuilding.workers.Count : -1;
    }

    public void AssignRaiderIndex()
    {
        raiderIndex = interactBuilding ? interactBuilding.workers.Count : -1;
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