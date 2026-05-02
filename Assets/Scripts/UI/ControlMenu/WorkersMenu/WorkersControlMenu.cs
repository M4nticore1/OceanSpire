using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkersControlMenu : ControlMenu
{
    [SerializeField] private WorkersPanel buildingWorkersMenu;
    [SerializeField] private WorkersPanel unemployedCitizensMenu;
    [SerializeField] private WorkersPanel employedCitizensMenu;
    [SerializeField] private RectTransform scrollRectContent;

    protected override void OnEnable()
    {
        base.OnEnable();

        Human.onHumanInited += OnHumanInited;
        Human.onHumanDied += OnHumanDied;

        InteractComponent.onInteractorSetedInteractBuilding += OnSetedCitizenWork;
        InteractComponent.onInteractorRemovedInteractBuilding += OnRemovedCitizenWork;
    }

    protected override void OnDisable()
    {
        base.OnEnable();

        Human.onHumanInited -= OnHumanInited;
        Human.onHumanDied -= OnHumanDied;

        InteractComponent.onInteractorSetedInteractBuilding -= OnSetedCitizenWork;
        InteractComponent.onInteractorRemovedInteractBuilding -= OnRemovedCitizenWork;
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
        if (!selectedBuilding) return;

        int maxWorkersCount = selectedBuilding.LevelData.maxResidentsCount;

        buildingWorkersMenu.ClearWidgets();
        employedCitizensMenu.ClearWidgets();
        unemployedCitizensMenu.ClearWidgets();

        List<Human> citizens = CreaturesManager.Instance.Citizens.ToList();

        foreach (var citizen in citizens) {
            if (!citizen.HealthComponent.IsAlive) continue;

            if (citizen.InteractComponent.InteractBuilding == selectedBuilding) {
                buildingWorkersMenu.CreateWidget(citizen);
            }
            else if (citizen.InteractComponent.InteractBuilding) {
                employedCitizensMenu.CreateWidget(citizen);
            }
            else {
                unemployedCitizensMenu.CreateWidget(citizen);
            }
        }

        while (buildingWorkersMenu.SpawnedWidgets.Count < maxWorkersCount) {
            buildingWorkersMenu.CreateWidget(null);
        }

        buildingWorkersMenu.UpdateMenu();
        employedCitizensMenu.UpdateMenu();
        unemployedCitizensMenu.UpdateMenu();

        UpdateScrollRect();
    }

    private void TryUpdateMenu(Human human)
    {
        if (!ShouldUpdateMenu(human)) return;

        UpdateMenu();
    }

    private void UpdateScrollRect()
    {
        var buildingWorkersRect = buildingWorkersMenu.GetComponent<RectTransform>();
        var employedCitizensRect = employedCitizensMenu.GetComponent<RectTransform>();

        float width = scrollRectContent.sizeDelta.x;
        float height = employedCitizensRect.rect.position.y - buildingWorkersRect.rect.position.y + employedCitizensRect.rect.size.y;

        scrollRectContent.sizeDelta = new Vector2(width, height);
    }

    private void OnSetedCitizenWork(InteractComponent interactor)
    {
        TryUpdateMenu(interactor.GetComponent<Human>());
    }

    private void OnRemovedCitizenWork(InteractComponent interactor)
    {
        TryUpdateMenu(interactor.GetComponent<Human>());
    }

    private void OnHumanInited(Human human)
    {
        TryUpdateMenu(human);
    }

    private void OnHumanDied(Human human)
    {
        TryUpdateMenu(human);
    }

    private bool ShouldUpdateMenu(Human human)
    {
        if (!isOpened) return false;
        if (human.currentStatusEnum != HumanStatusEnum.Citizen) return false;

        return true;
    }
}