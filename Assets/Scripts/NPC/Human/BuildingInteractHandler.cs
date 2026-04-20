using System;
using UnityEngine;

public class BuildingInteractHandler : MonoBehaviour
{
    public Building interactBuilding { get; private set; }

    public bool isInteracting { get; private set; } = false;
    public int workerIndex { get; private set; } = 0;
    public int raiderIndex { get; private set; } = 0;

    public event Action onSetedInteractBuilding;
    public event Action onRemovedInteractBuilding;
    public event Action onStartedInteracting;
    public event Action onStoppedInteracting;

    private void Update()
    {
        if (isInteracting) {
            Interacting();
        }
    }

    public void SetInteractBuilding(Building building)
    {
        interactBuilding = building;
        onSetedInteractBuilding?.Invoke();
    }

    public void RemoveInteractBuilding()
    {
        if (isInteracting) {
            StopInteracting();
        }

        interactBuilding = null;
        onRemovedInteractBuilding?.Invoke();
    }

    public void StartInteracting()
    {
        interactBuilding.AddCurrentWorker(this);
        isInteracting = true;
        onStartedInteracting?.Invoke();
    }

    // Events
    private void OnWorkerWidgetClicked(CitizenWidget widget)
    {
        Human resident = widget.Human;
        if (resident != GetComponent<Human>()) return;

        Building selectedBuilding = SelectManager.Instance.GetSelectedBuilding();

        if (interactBuilding) {
            if (selectedBuilding == interactBuilding) {
                RemoveInteractBuilding();
            }
            else {
                if (selectedBuilding.workers.Count < selectedBuilding.LevelsData[selectedBuilding.LevelComponent.level].maxResidentsCount) {
                    RemoveInteractBuilding();
                    SetInteractBuilding(selectedBuilding);
                }
            }
        }
        else {
            if (selectedBuilding.workers.Count < selectedBuilding.LevelsData[selectedBuilding.LevelComponent.level].maxResidentsCount) {
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
        onStoppedInteracting?.Invoke();
    }

    private void Interacting()
    {

    }
}