using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkersMenuManager : UIBehaviour
{
    [SerializeField] private EntitiesManager entitiesManager;

    private bool isOpened = false;

    [SerializeField] private RectTransform content = null;

    [SerializeField] private WorkersMenu buildingWorkersMenu = null;
    [SerializeField] private WorkersMenu unemployedCitizensMenu = null;
    [SerializeField] private WorkersMenu employedCitizensMenu = null;

    [SerializeField] private CustomButton closeMenuButton = null;

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

        EventBus.onClickedWorkersButton += OnClickedWorkersButton;
        EventBus.onCitizenInited += OnCitizenAdded;
        EventBus.onSetedInteractBuilding += OnSetedCitizenWork;
        EventBus.onRemovedInteractBuilding += OnRemovedCitizenWork;
        closeMenuButton.onReleased += Close;
    }

    protected override void OnDisable()
    {
        base.OnEnable();

        EventBus.onClickedWorkersButton -= OnClickedWorkersButton;
        EventBus.onCitizenInited -= OnCitizenAdded;
        EventBus.onSetedInteractBuilding -= OnSetedCitizenWork;
        EventBus.onRemovedInteractBuilding -= OnRemovedCitizenWork;
        closeMenuButton.onReleased -= Close;
    }

    protected override void Start()
    {
        base.Start();

        Close();
    }

    // Open / Close
    private void Open()
    {
        content.gameObject.SetActive(true);
        UpdateWorkersMenu();
        isOpened = true;
    }

    private void Close()
    {
        content.gameObject.SetActive(false);
        isOpened = false;
        EventBus.InvokeWorkersMenuClosed();
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
