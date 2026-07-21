using System.Collections.Generic;
using UnityEngine;

public class WorkersControlMenu : ControlMenu
{
    [Header("Workers")]
    [Header("Main")]
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private SelectManager selectManager;

    [Header("UI")]
    [SerializeField] private CitizenWidget citizenWidgetPrefab;
    [SerializeField] private WorkersPanel buildingWorkersMenu;
    [SerializeField] private WorkersPanel unemployedCitizensMenu;
    [SerializeField] private WorkersPanel employedCitizensMenu;
    [SerializeField] private RectTransform scrollRectContent;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    private Dictionary<Citizen, CitizenWidget> spawnedWidgets = new();
    private List<CitizenWidget> spawnedEmptyWidgets = new();

    protected override void Subscribe()
    {
        base.Subscribe();

        Human.OnHumanInited += OnHumanInited;
        Human.OnHumanDied += OnHumanDied;
        Citizen.OnCitizenEvicted += OnCitizenEvicted;

        CreatureInteractComponent.OnInteractorInteractBuildingSeted += OnCitizenWorkSeted;
        CreatureInteractComponent.OnInteractorInteractBuildirngRemoved += OnCitizenWorkRemoved;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        Human.OnHumanInited -= OnHumanInited;
        Human.OnHumanDied -= OnHumanDied;
        Citizen.OnCitizenEvicted -= OnCitizenEvicted;

        CreatureInteractComponent.OnInteractorInteractBuildingSeted -= OnCitizenWorkSeted;
        CreatureInteractComponent.OnInteractorInteractBuildirngRemoved -= OnCitizenWorkRemoved;
    }

    protected override void OnShow()
    {
        UpdateMenus();
        InputStateManager.Instance.AddBlockTarget();
    }

    protected override void OnHide()
    {
        InputStateManager.Instance.RemoveBlockTarget();
    }

    private void TryUpdateMenu(Human human)
    {
        if (!ShouldUpdateMenu(human)) return;

        UpdateMenu();
    }

    protected override void UpdateMenu()
    {
        if (!creaturesManager) return;
        if (!selectManager) return;

        var selectedBuilding = selectManager.GetSelectedBuilding();
        if (!selectedBuilding) {
            Debug.LogError("[{nameof(WorkersControlMenu)}] SelectedBuilding is not valid");
            return;
        }

        int maxWorkersCount = selectedBuilding.LevelDefinition.MaxHumansCount;

        for (int i = spawnedEmptyWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedEmptyWidgets[i];
            if (!widget) {
                Debug.LogError($"[{nameof(WorkersControlMenu)}] Spawned Empty Widget is not valid!");
                continue;
            }

            RemoveWidget(widget);
        }

        foreach (var widget in spawnedWidgets.Values) {
            if (!widget) {
                Debug.LogError($"[{nameof(WorkersControlMenu)}] Spawned Widget is not valid!");
                continue;
            }

            var citizen = widget.Citizen;
            if (!citizen) continue;

            if (!citizen.IsCitizenAvaliable()) continue;

            var interactBuilding = citizen.InteractComponent.InteractBuilding;

            if (interactBuilding == selectedBuilding) {
                widget.transform.SetParent(buildingWorkersMenu.LayoutGroup.transform);
            }
            else if (interactBuilding) {
                widget.transform.SetParent(employedCitizensMenu.LayoutGroup.transform);
            }
            else {
                widget.transform.SetParent(unemployedCitizensMenu.LayoutGroup.transform);
            }
        }

        while (buildingWorkersMenu.LayoutGroup.transform.childCount < maxWorkersCount) {
            CreateWidget();
        }

        buildingWorkersMenu.UpdateMenu();
        employedCitizensMenu.UpdateMenu();
        unemployedCitizensMenu.UpdateMenu();
        UpdateScrollRect();
    }

    private void UpdateScrollRect()
    {
        fitSizeToChildren.UpdateSize();
    }

    private void CreateWidget()
    {
        var widget = CitizenWidgetFactory.CreateWidget(citizenWidgetPrefab, buildingWorkersMenu.LayoutGroup.transform, null);
        spawnedEmptyWidgets.Add(widget);
    }

    private void CreateWidget(Citizen citizen)
    {
        if (spawnedWidgets.ContainsKey(citizen)) return;
        if (!citizen.IsCitizenAvaliable()) return;

        var selectedBuilding = selectManager.GetSelectedBuilding();
        Transform widgetTransform = null;

        if (selectedBuilding) {
            var interactBuilding = citizen.InteractComponent.InteractBuilding;

            if (interactBuilding == selectedBuilding) {
                widgetTransform = buildingWorkersMenu.LayoutGroup.transform;
            }
            else if (interactBuilding) {
                widgetTransform = employedCitizensMenu.LayoutGroup.transform;
            }
            else {
                widgetTransform = unemployedCitizensMenu.LayoutGroup.transform;
            }
        }
        else {
            widgetTransform = unemployedCitizensMenu.LayoutGroup.transform;
        }

        var widget = CitizenWidgetFactory.CreateWidget(citizenWidgetPrefab, widgetTransform, citizen);
        spawnedWidgets.Add(citizen, widget);
    }

    private void RemoveWidget(CitizenWidget citizenWidget)
    {
        if (!citizenWidget) return;

        Destroy(citizenWidget.gameObject);
        citizenWidget.transform.SetParent(null);
        spawnedEmptyWidgets.Remove(citizenWidget);
    }

    private void RemoveWidget(Citizen citizen)
    {
        if (!spawnedWidgets.TryGetValue(citizen, out var widget)) return;

        Destroy(widget.gameObject);
        spawnedWidgets.Remove(citizen);
    }

    private void UpdateMenus()
    {
        buildingWorkersMenu.UpdateMenu();
        employedCitizensMenu.UpdateMenu();
        unemployedCitizensMenu.UpdateMenu();
    }

    private void OnCitizenWorkSeted(CreatureInteractComponent interactor)
    {
        TryUpdateMenu(interactor.GetComponent<Human>());
    }

    private void OnCitizenWorkRemoved(CreatureInteractComponent interactor)
    {
        TryUpdateMenu(interactor.GetComponent<Human>());
    }

    private void OnHumanInited(Human human)
    {
        var citizen = human as Citizen;
        if (!citizen) return;

        CreateWidget(citizen);
        UpdateMenus();
    }

    private void OnHumanDied(Human human)
    {
        var citizen = human as Citizen;
        if (!citizen) return;

        RemoveWidget(citizen);
        UpdateMenus();
    }

    private void OnCitizenEvicted(Citizen citizen)
    {
        CreateWidget(citizen);
        UpdateMenus();
    }

    private bool ShouldUpdateMenu(Human human)
    {
        if (!human) return false;
        if (!isOpened) return false;

        var citizen = human as Citizen;
        if (!citizen) return false;

        return true;
    }
}