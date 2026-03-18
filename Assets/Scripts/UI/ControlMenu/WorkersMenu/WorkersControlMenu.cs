using System.Collections.Generic;
using UnityEngine;

public class WorkersControlMenu : ControlMenu
{
    [SerializeField] private EntitiesManager entitiesManager;

    [SerializeField] private WorkersPanel buildingWorkersMenu;
    [SerializeField] private WorkersPanel unemployedCitizensMenu;
    [SerializeField] private WorkersPanel employedCitizensMenu;

    protected override void Awake()
    {
        base.Awake();

        buildingWorkersMenu.Init();
        unemployedCitizensMenu.Init();
        employedCitizensMenu.Init();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onCitizenInited += OnCitizenAdded;
        EventBus.onSetedInteractBuilding += OnSetedCitizenWork;
        EventBus.onRemovedInteractBuilding += OnRemovedCitizenWork;
    }

    protected override void OnDisable()
    {
        base.OnEnable();

        EventBus.onCitizenInited -= OnCitizenAdded;
        EventBus.onSetedInteractBuilding -= OnSetedCitizenWork;
        EventBus.onRemovedInteractBuilding -= OnRemovedCitizenWork;
    }

    protected override void Start()
    {
        base.Start();

        UpdateWorkersMenu();
    }

    protected override void OnOpen()
    {
        
    }

    protected override void OnClose()
    {

    }

    private void UpdateWorkersMenu()
    {
        Building selectedBuilding = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        List<Human> citizens = entitiesManager.citizens;
        int maxWorkersCount = selectedBuilding.LevelData.maxResidentsCount;

        buildingWorkersMenu.ClearWidgets();
        employedCitizensMenu.ClearWidgets();
        unemployedCitizensMenu.ClearWidgets();

        for (int i = 0; i < citizens.Count; i++) {
            Human citizen = citizens[i];

            if (citizen.interactor.InteractBuilding == selectedBuilding) {
                buildingWorkersMenu.CreateWidget(citizen);
            }
            else if (citizen.interactor.InteractBuilding) {
                employedCitizensMenu.CreateWidget(citizen);
            }
            else {
                unemployedCitizensMenu.CreateWidget(citizen);
            }
        }

        while (buildingWorkersMenu.SpawnedCitizenWidgets.Count < maxWorkersCount) {
            buildingWorkersMenu.CreateWidget(null);
        }
    }

    // Events
    private void OnClickedWorkersButton()
    {
        Open();
    }

    private void OnSetedCitizenWork()
    {
        if (!isOpened) return;

        UpdateWorkersMenu();
    }

    private void OnRemovedCitizenWork()
    {
        if (!isOpened) return;

        UpdateWorkersMenu();
    }

    private void OnCitizenAdded(Human resident)
    {
        if (!isOpened) return;

        UpdateWorkersMenu();
    }
}
