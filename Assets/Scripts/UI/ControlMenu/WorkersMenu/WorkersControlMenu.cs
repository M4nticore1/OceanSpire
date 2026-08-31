using System.Collections.Generic;
using UnityEngine;

public class WorkersControlMenu : ControlMenu
{
    public static WorkersControlMenu Instance { get; private set; }

    [Header("Workers Menu")]
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

    private List<CitizenWidget> spawnedWidgets = new();
    private List<CitizenWidget> spawnedEmptyWidgets = new();

    private Building currentBuilding;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null) {
            Debug.LogError($"[{nameof(WorkersControlMenu)}] There's another Workers Control Menu on the scene!");
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

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
        base.OnShow();

        UpdateMenu();
        UpdateWidgetsSort();
        UpdateWidgetsHighlight();

        UpdatePanelSizes();
        UpdateScrollRect();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void UpdateMenu()
    {
        if (creaturesManager == null) return;
        if (selectManager == null) return;

        if (currentBuilding == null) {
            Debug.LogError($"[{nameof(WorkersControlMenu)}] Current Building is not valid");
            return;
        }

        int maxWorkersCount = currentBuilding.LevelDefinition.MaxHumansCount;
        for (int i = spawnedEmptyWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedEmptyWidgets[i];
            if (widget == null) {
                Debug.LogError($"[{nameof(WorkersControlMenu)}] Spawned Empty Widget is not valid!");
                continue;
            }

            RemoveWidget(widget);
        }

        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedWidgets[i];
            if (widget == null) {
                Debug.LogError($"[{nameof(WorkersControlMenu)}] Spawned Widget is not valid!");
                continue;
            }

            var citizen = widget.Citizen;
            if (citizen == null) continue;

            if (!citizen.IsCitizenAvailable()) {
                RemoveWidget(widget);
                continue;
            }

            var interactBuilding = citizen.InteractComponent.InteractBuilding;
            WorkersPanel targetPanel;

            if (interactBuilding == currentBuilding) {
                targetPanel = buildingWorkersMenu;
            }
            else if (interactBuilding != null) {
                targetPanel = employedCitizensMenu;
            }
            else {
                targetPanel = unemployedCitizensMenu;
            }

            if (widget.transform.parent != targetPanel.LayoutGroup.transform) {
                buildingWorkersMenu.RemoveWidget(widget);
                employedCitizensMenu.RemoveWidget(widget);
                unemployedCitizensMenu.RemoveWidget(widget);

                widget.transform.SetParent(targetPanel.LayoutGroup.transform);
                targetPanel.AddWidget(widget);
            }
        }

        while (buildingWorkersMenu.LayoutGroup.transform.childCount < maxWorkersCount) {
            CreateWidget();
        }

        buildingWorkersMenu.UpdateMenu();
        employedCitizensMenu.UpdateMenu();
        unemployedCitizensMenu.UpdateMenu();
    }

    protected override ILocalizable GetTargetNameText()
    {
        return currentBuilding;
    }

    protected override ILocalizable GetTargetDescriptionText()
    {
        return currentBuilding;
    }

    public void Show(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(WorkersControlMenu)}] Building is not valid!");
            return;
        }

        currentBuilding = building;
        SetWidgetsInteractBuilding(building);
        Show();
    }

    private void UpdateScrollRect()
    {
        fitSizeToChildren.UpdateSize();
    }

    private void UpdatePanelSizes()
    {
        buildingWorkersMenu.UpdateSize();
        employedCitizensMenu.UpdateSize();
        unemployedCitizensMenu.UpdateSize();
    }

    private void UpdateWidgetsSort()
    {
        //buildingWorkersMenu.SortWidgets();
        employedCitizensMenu.SortWidgets(currentBuilding);
        unemployedCitizensMenu.SortWidgets(currentBuilding);
    }

    private void UpdateWidgetsHighlight()
    {
        if (!currentBuilding) {
            Debug.LogError($"[{nameof(WorkersControlMenu)}] Selected Building is not valid!");
            return;
        }

        var buildingSkillId = currentBuilding.SkillId;

        foreach (var citizenWidget in spawnedWidgets) {
            if (citizenWidget == null) continue;

            foreach (var skillWidget in citizenWidget.SkillsPanel.SpawnedSkillWidgets) {
                if (skillWidget == null) continue;

                skillWidget.SetHighlighted(skillWidget.Skill.SkillDefinition.SkillId == buildingSkillId);
            }
        }
    }

    private void CreateWidget()
    {
        var widget = CitizenWidgetFactory.CreateWidget(citizenWidgetPrefab, buildingWorkersMenu.LayoutGroup.transform, null);
        spawnedEmptyWidgets.Add(widget);
        buildingWorkersMenu.AddWidget(widget);
    }

    private void CreateWidget(Citizen citizen)
    {
        if (citizen == null) return;
        if (!citizen.IsCitizenAvailable()) return;

        var selectedBuilding = selectManager.GetSelectedBuilding();
        WorkersPanel menu = null;

        if (selectedBuilding) {
            var interactBuilding = citizen.InteractComponent.InteractBuilding;

            if (interactBuilding == selectedBuilding) {
                menu = buildingWorkersMenu;
            }
            else if (interactBuilding) {
                menu = employedCitizensMenu;
            }
            else {
                menu = unemployedCitizensMenu;
            }
        }
        else {
            menu = unemployedCitizensMenu;
        }

        var widget = CitizenWidgetFactory.CreateWidget(citizenWidgetPrefab, menu.LayoutGroup.transform, citizen);
        spawnedWidgets.Add(widget);

        menu.AddWidget(widget);
        menu.SortWidgets(currentBuilding);
    }

    private void RemoveWidget(CitizenWidget widget)
    {
        if (widget == null) return;

        spawnedWidgets.Remove(widget);
        spawnedEmptyWidgets.Remove(widget);

        buildingWorkersMenu.RemoveWidget(widget);
        employedCitizensMenu.RemoveWidget(widget);
        unemployedCitizensMenu.RemoveWidget(widget);

        widget.transform.SetParent(null);
        Destroy(widget.gameObject);

        UpdateWidgetsSort();
    }

    private void UpdateMenus()
    {
        buildingWorkersMenu.UpdateMenu();
        employedCitizensMenu.UpdateMenu();
        unemployedCitizensMenu.UpdateMenu();
    }

    private void SetWidgetsInteractBuilding(Building building)
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedWidgets[i];
            if (widget == null) {
                spawnedWidgets.RemoveAt(i);
                continue;
            }

            widget.SetInteractBuilding(building);
        }
    }

    private void OnCitizenWorkSeted(CreatureInteractComponent interactor)
    {
        var human = interactor.GetComponent<Human>();
        if (TryUpdateMenu(human)) {
            UpdateWidgetsSort();
        }
    }

    private void OnCitizenWorkRemoved(CreatureInteractComponent interactor)
    {
        var human = interactor.GetComponent<Human>();
        if (TryUpdateMenu(human)) {
            UpdateWidgetsSort();
        }
    }

    private void OnHumanInited(Human human)
    {
        var citizen = human as Citizen;
        if (citizen == null) return;

        CreateWidget(citizen);
        UpdateMenus();
    }

    private void OnHumanDied(Human human)
    {
        var citizen = human as Citizen;
        if (!citizen) return;

        var widget = GetSpawnedWidgetByCitizen(citizen);
        if (widget == null) return;

        RemoveWidget(widget);
        UpdateMenus();
    }

    private void OnCitizenEvicted(Citizen citizen)
    {
        CreateWidget(citizen);
        UpdateMenus();
    }

    private bool TryUpdateMenu(Human human)
    {
        if (!ShouldUpdateMenu(human)) return false;

        UpdateMenu();
        return true;
    }

    private bool ShouldUpdateMenu(Human human)
    {
        if (!IsShowed) return false;
        if (human == null) return false;

        var citizen = human as Citizen;
        if (citizen == null) return false;

        return true;
    }

    private CitizenWidget GetSpawnedWidgetByCitizen(Citizen citizen)
    {
        foreach (var widget in spawnedWidgets) {
            if (widget == null) continue;
            if (widget.Citizen == citizen) return widget;
        }

        return null;
    }
}