using System;
using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
public class EntityInteractor : MonoBehaviour
{
    private EntityMovement movement = null;

    [SerializeField] private Building interactBuilding = null;
    public Building InteractBuilding => interactBuilding;

    public bool isInteracting { get; private set; } = false;
    public int interacterIndex { get; private set; } = 0;

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
        EventBus.onResidentWidgetClicked += OnWorkerWidgetClicked;
    }

    private void OnDisable()
    {
        EventBus.onResidentWidgetClicked -= OnWorkerWidgetClicked;
    }

    private void Update()
    {
        if (isInteracting) {
            Interacting();
        }
    }

    // Events
    private void OnWorkerWidgetClicked(ResidentWidget widget)
    {
        Human resident = widget.resident;
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

    public void SetInteractBuilding(Building building)
    {
        if (!building) {
            Debug.LogError("building is null.");
            return;
        }

        interactBuilding = building;
        onSetedInteractBuilding?.Invoke(building);
    }

    private void RemoveInteractBuilding()
    {
        if (isInteracting)
            StopInteractingBuilding();
        onRemovedInteractBuilding?.Invoke(interactBuilding);
        interactBuilding = null;
    }

    public void SetInteracterIndex(int index)
    {
        interacterIndex = index;
    }

    public void StartInteractingBuilding()
    {
        isInteracting = true;
        movement.MoveTo(InteractBuilding.GetInteractionTransform().position);
        onStartedInteracting?.Invoke(interactBuilding);
    }

    private void StopInteractingBuilding()
    {
        if (!isInteracting) return;

        isInteracting = false;
        onStoppedInteracting?.Invoke(interactBuilding);
    }

    private void Interacting()
    {
        if (interactBuilding.GetComponent<PierModule>())
            return;

        if (interactBuilding.spawnedConstruction.BuildingInteractions.Length > interacterIndex) {
            BuildingAction buildingAction = interactBuilding.spawnedConstruction.BuildingInteractions[interacterIndex];

            if (buildingAction.actionTimes[currentActionIndex] > 0) {
                currentActionTime += Time.deltaTime;
                if (currentActionTime >= buildingAction.actionTimes[currentActionIndex]) {
                    if (currentActionIndex < buildingAction.actionTimes.Length - 1)
                        currentActionIndex++;
                    else
                        currentActionIndex = 0;

                    currentActionTime = 0;

                    Vector3 position = buildingAction.waypoints[currentActionIndex].position;
                    movement.MoveTo(position);
                }
            }
        }
    }
}