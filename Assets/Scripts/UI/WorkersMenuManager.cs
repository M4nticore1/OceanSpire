using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkersMenuManager : UIBehaviour
{
    private bool isOpened = false;

    [SerializeField] private RectTransform content = null;

    [SerializeField] private WorkersMenu buildingWorkersMenu = null;
    [SerializeField] private WorkersMenu unemployedCitizensMenu = null;
    [SerializeField] private WorkersMenu employedCitizensMenu = null;

    [SerializeField] private CustomButton closeMenuButton = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onCitizenAdded += OnCitizenAdded;
        EventBus.onSetedInteractBuilding += OnSetedCitizenWork;
        EventBus.onRemovedInteractBuilding += OnRemovedCitizenWork;
        closeMenuButton.onReleased += CloseWorkersMenu;
    }

    protected override void OnDisable()
    {
        base.OnEnable();

        EventBus.onCitizenAdded -= OnCitizenAdded;
        EventBus.onSetedInteractBuilding -= OnSetedCitizenWork;
        EventBus.onRemovedInteractBuilding -= OnRemovedCitizenWork;
        closeMenuButton.onReleased -= CloseWorkersMenu;
    }

    // Open / Close
    public void OpenWorkersMenu()
    {
        content.gameObject.SetActive(true);
        UpdateWorkersMenu();
        isOpened = true;
    }

    public void CloseWorkersMenu()
    {
        content.gameObject.SetActive(false);
        isOpened = false;
    }

    private void UpdateWorkersMenu()
    {
        Building selectedBuilding = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        List<Human> citizens = CityManager.Instance.citizens;
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

    // Update Widgets
    //private void UpdateWidgets()
    //{
    //    Building selectedBuilding = SelectManager.Instance.selectedComponent.GetComponent<Building>();
    //    List<Human> citizens = CityManager.Instance.citizens;
    //    int buildingCitizenWidgetsCount = 0;
    //    int employedCitizenWidgetsCount = 0;
    //    int unemployedCitizenWidgetsCount = 0;

    //    for (int i = 0; i < citizens.Count; i++) {
    //        Human citizen = citizens[i];

    //        if (selectedBuilding.workers.Contains(citizen)) {
    //            buildingWorkersMenu.
    //            //buildingCitizenWidgetsCount++;
    //            //UpdateList(spawnedBuildingCitizenWidgets, buildingWorkersList, buildingCitizenWidgetsCount, citizen);
    //        }
    //        else if (citizen.interactor.InteractBuilding) {
    //            //employedCitizenWidgetsCount++;
    //            //UpdateList(spawnedEmployedCitizenWidgets, employedCitizensList, employedCitizenWidgetsCount, citizen);
    //        }
    //        else {
    //            //unemployedCitizenWidgetsCount++;
    //            //UpdateList(spawnedUnemployedCitizenWidgets, unemployedCitizensList, unemployedCitizenWidgetsCount, citizen);
    //        }
    //    }

    //    AddMissingSelectedBuildingWidgets();
    //    RemoveExtraWidgets(spawnedBuildingCitizenWidgets, buildingCitizenWidgetsCount);
    //    RemoveExtraWidgets(spawnedEmployedCitizenWidgets, employedCitizenWidgetsCount);
    //    RemoveExtraWidgets(spawnedUnemployedCitizenWidgets, unemployedCitizenWidgetsCount);
    //}

    //private void AddMissingSelectedBuildingWidgets()
    //{
    //    Building selectedBuilding = SelectManager.Instance.selectedComponent.GetComponent<Building>();
    //    int maxWorkersCount = selectedBuilding.LevelData.maxResidentsCount;

    //    while (spawnedBuildingCitizenWidgets.Count < maxWorkersCount) {
    //        CitizenWidget spawnedWidget = Instantiate(citizenWidgetPrefab, buildingWorkersList.transform);
    //        spawnedBuildingCitizenWidgets.Add(spawnedWidget);
    //    }
    //}

    //private void RemoveExtraWidgets(List<CitizenWidget> list, int targetCount)
    //{
    //    while (list.Count > targetCount) {
    //        int index = list.Count - 1;
    //        Destroy(list[index]);
    //        list.RemoveAt(index);
    //    }
    //}

    //private void UpdateList(List<CitizenWidget> list, GridLayoutGroup layoutGroup, int currentCount, Human citizen)
    //{
    //    if (list.Count < currentCount) {
    //        CitizenWidget spawnedWidget = Instantiate(citizenWidgetPrefab, layoutGroup.transform);
    //        list.Add(spawnedWidget);
    //    }

    //    CitizenWidget widget = list[currentCount - 1];
    //    widget.SetCitizen(citizen);
    //}

    //// Update Size
    //private void UpdateListsSize()
    //{
    //    Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
    //    if (!building) return;

    //    // Building workers
    //    SetWorkerListSize(buildingWorkersMenu, buildingWorkersList, null, spawnedBuildingCitizenWidgets.Count);
    //    SetWorkerListSize(unemployedCitizensMenu, unemployedCitizensList, haveNoUnemployedCitizensText, spawnedUnemployedCitizenWidgets.Count);
    //    SetWorkerListSize(employedCitizensMenu, employedCitizensList, haveNoEnployedCitizensText, spawnedEmployedCitizenWidgets.Count);
    //}

    //private void SetWorkerListSize(RectTransform workersMenu, GridLayoutGroup gridLayoutGroup, GameObject haveNoResidentsText, int residentsCount)
    //{
    //    if (residentsCount == 0) {

    //    }
    //    else {
    //        int columnsCount = gridLayoutGroup.constraintCount;
    //        RectTransform menuRectTransform = gridLayoutGroup.GetComponent<RectTransform>();
    //        int rowsCount = (int)math.ceil((float)residentsCount / columnsCount);
    //    }
    //}

    // Events
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
