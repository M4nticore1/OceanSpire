using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkersControlMenu : ControlMenu
{
    [SerializeField] private WorkersPanel buildingWorkersMenu;
    [SerializeField] private WorkersPanel unemployedCitizensMenu;
    [SerializeField] private WorkersPanel employedCitizensMenu;
    [SerializeField] private RectTransform scrollRectContent;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    protected override void OnEnable()
    {
        base.OnEnable();

        Human.OnHumanInited += OnHumanInited;
        Human.OnHumanDied += OnHumanDied;

        BuildingInteractComponent.OnInteractorInteractBuildingSeted += OnCitizenWorkSeted;
        BuildingInteractComponent.OnInteractorInteractBuildirngRemoved += OnCitizenWorkRemoved;
    }

    protected override void OnDisable()
    {
        base.OnEnable();

        Human.OnHumanInited -= OnHumanInited;
        Human.OnHumanDied -= OnHumanDied;

        BuildingInteractComponent.OnInteractorInteractBuildingSeted -= OnCitizenWorkSeted;
        BuildingInteractComponent.OnInteractorInteractBuildirngRemoved -= OnCitizenWorkRemoved;
    }

    protected override void OnOpen()
    {
       
    }

    protected override void OnClose()
    {

    }

    protected override void UpdateMenu()
    {
        var selectedBuilding = SelectManager.Instance.GetSelectedBuilding();
        if (!selectedBuilding) return;

        int maxWorkersCount = selectedBuilding.LevelData.MaxHumansCount;

        buildingWorkersMenu.ClearWidgets();
        employedCitizensMenu.ClearWidgets();
        unemployedCitizensMenu.ClearWidgets();

        var citizens = CreaturesManager.Instance.Citizens.ToList();

        foreach (var citizen in citizens) {
            if (!citizen.IsCitizenAvaliable()) continue;

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
        fitSizeToChildren.UpdateSize();
    }

    private void OnCitizenWorkSeted(BuildingInteractComponent interactor)
    {
        TryUpdateMenu(interactor.GetComponent<Human>());
    }

    private void OnCitizenWorkRemoved(BuildingInteractComponent interactor)
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

        var citizen = human.GetComponent<Citizen>();
        if (!citizen) return false;

        return true;
    }
}