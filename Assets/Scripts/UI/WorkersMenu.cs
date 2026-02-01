using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkersMenu : UIBehaviour
{
    [SerializeField] private ResidentWidget residentWidgetPrefab = null;

    private List<ResidentWidget> spawnedBuildingWorkerEmptyWidgets = new List<ResidentWidget>();
    private List<ResidentWidget> spawnedResidentWidgets = new List<ResidentWidget>();

    [SerializeField] private RectTransform buildingWorkersMenu = null;
    [SerializeField] private RectTransform unemployedResidentsMenu = null;
    [SerializeField] private RectTransform workingResidentsMenu = null;

    [SerializeField] private RectTransform haveNoUnemployedResidentsText = null;
    [SerializeField] private RectTransform haveNoWorkingResidentsText = null;

    [SerializeField] private GridLayoutGroup buildingWorkersList = null;
    [SerializeField] private GridLayoutGroup unemployedResidentsList = null;
    [SerializeField] private GridLayoutGroup workingResidentsList = null;

    [SerializeField] private CustomButton closeMenuButton = null;

    int maxBuildingWorkersCount = 0;
    int residentWidgetsColumnCount = 0;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onResidentWidgetClicked += OnResidentWidgetClicked;
        EventBus.onResidentAdded += OnResidentAdded;
        closeMenuButton.onReleased += CloseBuildingWorkersMenu;
    }

    protected override void OnDisable()
    {
        base.OnEnable();

        EventBus.onResidentWidgetClicked -= OnResidentWidgetClicked;
        EventBus.onResidentAdded -= OnResidentAdded;
        closeMenuButton.onReleased -= CloseBuildingWorkersMenu;
    }

    public void OpenWorkersMenu()
    {
        Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        if (!building) return;

        maxBuildingWorkersCount = building.ConstructionLevelsData[building.LevelIndex].maxResidentsCount;
        residentWidgetsColumnCount = (int)(buildingWorkersList.GetComponent<RectTransform>().rect.width / buildingWorkersList.cellSize.x);
        int residentsCount = CityManager.Instance.residents.Count;

        // Create Widgets
        while (spawnedResidentWidgets.Count < residentsCount) {
            CreateResidentWidget();
        }

        // Delete Extra Widgets
        while (spawnedResidentWidgets.Count > residentsCount) {
            int lastIndex = spawnedResidentWidgets.Count - 1;
            Destroy(spawnedResidentWidgets[lastIndex]);
            spawnedResidentWidgets.RemoveAt(lastIndex);
        }

        // Set parents of resident widgets
        int buildingWorkerWidgetIndex = 0;
        for (int i = 0; i < CityManager.Instance.residents.Count; i++) {
            spawnedResidentWidgets[i].InitializeResidentWidget(CityManager.Instance.residents[i]);

            if (CityManager.Instance.residents[i].workBuilding) {
                if (CityManager.Instance.residents[i].workBuilding == building) {
                    spawnedResidentWidgets[i].transform.SetParent(buildingWorkersList.transform);
                    spawnedResidentWidgets[i].transform.SetSiblingIndex(buildingWorkerWidgetIndex);
                    buildingWorkerWidgetIndex++;
                }
                else {
                    spawnedResidentWidgets[i].transform.SetParent(workingResidentsList.transform);
                }
            }
            else {
                spawnedResidentWidgets[i].transform.SetParent(unemployedResidentsList.transform);
            }
        }

        // Create empty resident widgets
        int emptyResidentWidgetsCount = spawnedBuildingWorkerEmptyWidgets.Count;
        if (emptyResidentWidgetsCount < maxBuildingWorkersCount) {
            for (int i = emptyResidentWidgetsCount; i < maxBuildingWorkersCount; i++) {
                ResidentWidget emptyResidentWidget = Instantiate(residentWidgetPrefab);
                emptyResidentWidget.InitializeResidentWidget(null);
                spawnedBuildingWorkerEmptyWidgets.Add(emptyResidentWidget);
                emptyResidentWidget.transform.SetParent(buildingWorkersList.transform);
                emptyResidentWidget.transform.localScale = Vector3.one;
            }
        }
        else {
            for (int i = emptyResidentWidgetsCount - 1; i >= maxBuildingWorkersCount; i--) {
                Destroy(spawnedBuildingWorkerEmptyWidgets[i].gameObject);
                spawnedBuildingWorkerEmptyWidgets.RemoveAt(i);
            }
        }

        for (int i = 0; i < building.workers.Count; i++) {
            spawnedBuildingWorkerEmptyWidgets[i].gameObject.SetActive(false);
        }

        for (int i = building.workers.Count; i < maxBuildingWorkersCount; i++) {
            spawnedBuildingWorkerEmptyWidgets[i].gameObject.SetActive(true);
        }

        UpdateWorkerListsSize();
    }

    private void OnResidentAdded(Creature resident)
    {
        CreateResidentWidget();
    }

    private void CreateResidentWidget()
    {
        ResidentWidget residentWidget = Instantiate(residentWidgetPrefab, unemployedResidentsList.transform);
        residentWidget.transform.localScale = Vector3.one;
        spawnedResidentWidgets.Add(residentWidget);
    }

    private void SetWorkerListSize(RectTransform workersMenu, GridLayoutGroup gridLayoutGroup, RectTransform haveNoResidentsText, int residentsCount, int residentWidgetsColumnCount)
    {
        gridLayoutGroup.constraintCount = residentWidgetsColumnCount;
        RectTransform menuRectTransform = gridLayoutGroup.GetComponent<RectTransform>();
        int WidgetsRowCount = (int)math.ceil((float)residentsCount / (float)residentWidgetsColumnCount);
        int YSize = 0;
        Vector2 ListSize = Vector2.zero;

        //Debug.Log(residentsCount);

        if (residentsCount > 0) {
            YSize = (int)((menuRectTransform.offsetMin.y - menuRectTransform.offsetMax.y) + (gridLayoutGroup.cellSize.y * WidgetsRowCount) + (gridLayoutGroup.spacing.y * (WidgetsRowCount - 1)));


            if (haveNoResidentsText)
                haveNoResidentsText.gameObject.SetActive(false);
        }
        else {
            YSize = (int)(menuRectTransform.offsetMin.y - menuRectTransform.offsetMax.y);

            if (haveNoResidentsText) {
                YSize += (int)haveNoResidentsText.sizeDelta.y;
                haveNoResidentsText.gameObject.SetActive(true);
            }
        }

        workersMenu.sizeDelta = new Vector2(workersMenu.sizeDelta.x, YSize);
    }

    private void UpdateWorkerListsSize()
    {
        Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        if (!building) return;

        // Building workers
        SetWorkerListSize(buildingWorkersMenu, buildingWorkersList, null, maxBuildingWorkersCount, residentWidgetsColumnCount);
        // Unemployed residents
        SetWorkerListSize(unemployedResidentsMenu, unemployedResidentsList, haveNoUnemployedResidentsText, CityManager.Instance.unemployedResidentsCount, residentWidgetsColumnCount);
        // Employed residents
        SetWorkerListSize(workingResidentsMenu, workingResidentsList, haveNoWorkingResidentsText, CityManager.Instance.employedResidentCount - building.workers.Count, residentWidgetsColumnCount);
    }

    private void OnResidentWidgetClicked(ResidentWidget widget)
    {
        Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        if (!building) return;

        Creature resident = widget.resident;
        int workersCount = building.workers.Count;

        if (resident.workBuilding) {
            if (resident.workBuilding == building) {
                widget.transform.SetParent(buildingWorkersList.transform);
                widget.transform.SetSiblingIndex(workersCount - 1);

                int index = building.workers.Count - 1;
                spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(false);
            }
            else {
                widget.transform.SetParent(workingResidentsList.transform);

                if (workersCount < maxBuildingWorkersCount) {
                    int index = maxBuildingWorkersCount - (maxBuildingWorkersCount - workersCount);
                    spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(true);
                }
            }
        }
        else {
            widget.transform.SetParent(unemployedResidentsList.transform);

            if (workersCount < maxBuildingWorkersCount) {
                int index = maxBuildingWorkersCount - (maxBuildingWorkersCount - workersCount);
                spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(true);
            }
        }

        UpdateWorkerListsSize();
    }

    private void CloseBuildingWorkersMenu()
    {
        gameObject.SetActive(false);
    }
}
