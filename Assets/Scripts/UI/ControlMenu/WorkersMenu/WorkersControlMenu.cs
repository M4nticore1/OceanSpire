using System.Collections.Generic;
using UnityEngine;

public class WorkersControlMenu : ControlMenu
{
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

        EventBus.onContextWorkersButtonClicked += OnContextWorkersButtonClicked;
        EventBus.onHumanInited += OnHumanInited;
        EventBus.onSetedWorkBuilding += OnSetedCitizenWork;
        EventBus.onRemovedWorkBuilding += OnRemovedCitizenWork;
    }

    protected override void OnDisable()
    {
        base.OnEnable();

        EventBus.onContextWorkersButtonClicked -= OnContextWorkersButtonClicked;
        EventBus.onHumanInited -= OnHumanInited;
        EventBus.onSetedWorkBuilding -= OnSetedCitizenWork;
        EventBus.onRemovedWorkBuilding -= OnRemovedCitizenWork;
    }

    protected override void OnOpen()
    {
       
    }

    protected override void OnClose()
    {

    }

    protected override void UpdateMenu()
    {
        Building selectedBuilding = SelectManager.Instance.GetSelectedBuilding();
        List<Human> citizens = CreaturesManager.instance.citizens;
        int maxWorkersCount = selectedBuilding.LevelData.maxResidentsCount;

        buildingWorkersMenu.ClearWidgets();
        employedCitizensMenu.ClearWidgets();
        unemployedCitizensMenu.ClearWidgets();

        for (int i = 0; i < citizens.Count; i++) {
            Human citizen = citizens[i];

            if (citizen.Interactor.interactBuilding == selectedBuilding) {
                buildingWorkersMenu.CreateWidget(citizen);
            }
            else if (citizen.Interactor.interactBuilding) {
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
    private void OnSetedCitizenWork()
    {
        if (!isOpened) return;

        UpdateMenu();
    }

    private void OnRemovedCitizenWork()
    {
        if (!isOpened) return;

        UpdateMenu();
    }

    private void OnHumanInited(Human resident)
    {
        if (!isOpened) return;

        UpdateMenu();
    }

    private void OnContextWorkersButtonClicked()
    {
        Open();
    }
}