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

    private Building selectedBuilding;

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
        selectedBuilding = selectManager.SelectedComponent?.GetComponent<Building>();

        UpdateMenu();
        UpdateWidgetsSort();
        UpdateWidgetsHighlight();
    }

    protected override void OnHide()
    {

    }

    protected override void UpdateMenu()
    {
        if (!creaturesManager) return;
        if (!selectManager) return;

        var selectedBuilding = selectManager.GetSelectedBuilding();
        if (!selectedBuilding) {
            Debug.LogError($"[{nameof(WorkersControlMenu)}] SelectedBuilding is not valid");
            return;
        }

        int maxWorkersCount = selectedBuilding.LevelDefinition.MaxHumansCount;

        // 1. Очищаем пустые виджеты
        for (int i = spawnedEmptyWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedEmptyWidgets[i];
            if (!widget) {
                Debug.LogError($"[{nameof(WorkersControlMenu)}] Spawned Empty Widget is not valid!");
                continue;
            }

            RemoveWidget(widget);
        }

        // 2. Перемещаем виджеты и ГАРАНТИРОВАННО синхронизируем их со списками панелей
        foreach (var widget in spawnedWidgets.Values) {
            if (!widget) {
                Debug.LogError($"[{nameof(WorkersControlMenu)}] Spawned Widget is not valid!");
                continue;
            }

            var citizen = widget.Citizen;
            if (!citizen) continue;

            if (!citizen.IsCitizenAvaliable()) continue;

            var interactBuilding = citizen.InteractComponent.InteractBuilding;
            WorkersPanel targetPanel;

            if (interactBuilding == selectedBuilding) {
                targetPanel = buildingWorkersMenu;
            }
            else if (interactBuilding) {
                targetPanel = employedCitizensMenu;
            }
            else {
                targetPanel = unemployedCitizensMenu;
            }

            // Переносим UI элемент и удаляем/добавляем в соответствующие списки списки панелей
            if (widget.transform.parent != targetPanel.LayoutGroup.transform) {
                // Убираем со ВСЕХ панелей на случай рассинхрона
                buildingWorkersMenu.RemoveWidget(widget);
                employedCitizensMenu.RemoveWidget(widget);
                unemployedCitizensMenu.RemoveWidget(widget);

                // Ставим нового родителя и добавляем в список целевой панели
                widget.transform.SetParent(targetPanel.LayoutGroup.transform);
                targetPanel.AddWidget(widget);
            }
        }

        // 3. Создаем пустые слоты
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

    private void UpdateWidgetsSort()
    {
        buildingWorkersMenu.SortWidgets();
        employedCitizensMenu.SortWidgets();
        unemployedCitizensMenu.SortWidgets();
    }

    private void UpdateWidgetsHighlight()
    {
        if (!selectedBuilding) {
            Debug.LogError($"[{nameof(WorkersControlMenu)}] Selected Building is not valid!");
            return;
        }

        var buildingSkillId = selectedBuilding.SkillId;

        foreach (var citizenWidget in spawnedWidgets.Values) {
            if (!citizenWidget) continue;

            foreach (var skillWidget in citizenWidget.SkillsPanel.SpawnedSkillWidgets) {
                if (!skillWidget) continue;

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
        if (spawnedWidgets.ContainsKey(citizen)) return;
        if (!citizen.IsCitizenAvaliable()) return;

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
        spawnedWidgets.Add(citizen, widget);

        menu.AddWidget(widget);
        menu.SortWidgets();
    }

    private void RemoveWidget(Citizen citizen)
    {
        if (!citizen) return;
        if (!spawnedWidgets.TryGetValue(citizen, out var citizenWidget)) return;

        RemoveWidget(citizenWidget);
    }

    private void RemoveWidget(CitizenWidget citizenWidget)
    {
        if (!citizenWidget) return;

        var citizen = citizenWidget.Citizen;

        if (citizen != null) {
            spawnedWidgets.Remove(citizen);
        }

        spawnedEmptyWidgets.Remove(citizenWidget);

        buildingWorkersMenu.RemoveWidget(citizenWidget);
        employedCitizensMenu.RemoveWidget(citizenWidget);
        unemployedCitizensMenu.RemoveWidget(citizenWidget);

        citizenWidget.transform.SetParent(null);
        Destroy(citizenWidget.gameObject);

        UpdateWidgetsSort();
    }

    private void UpdateMenus()
    {
        buildingWorkersMenu.UpdateMenu();
        employedCitizensMenu.UpdateMenu();
        unemployedCitizensMenu.UpdateMenu();
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

    private bool TryUpdateMenu(Human human)
    {
        if (!ShouldUpdateMenu(human)) return false;

        UpdateMenu();
        return true;
    }

    private bool ShouldUpdateMenu(Human human)
    {
        if (!human) return false;
        if (!IsShowed) return false;

        var citizen = human as Citizen;
        if (!citizen) return false;

        return true;
    }
}