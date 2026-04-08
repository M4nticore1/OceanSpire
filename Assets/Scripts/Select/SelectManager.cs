using UnityEngine;

public class SelectManager
{
    private static SelectManager _instance;
    public static SelectManager Instance => _instance ??= new SelectManager();

    private SelectManager()
    {
        EventBus.onSelectedComponent += OnSelectedComponent;
        EventBus.onDeselectedComponent += OnDeselectedComponent;
    }

    private SelectComponent selectedComponent;

    public void Deselect()
    {
        selectedComponent.Deselect();
    }

    public Building GetSelectedBuilding()
    {
        return selectedComponent.GetComponent<BuildingConstruction>().ownedBuilding;
    }

    public Human GetSelectedHuman()
    {
        return selectedComponent.GetComponent<Human>();
    }

    private void OnSelectedComponent(SelectComponent component)
    {
        selectedComponent = component;
    }

    private void OnDeselectedComponent(SelectComponent component)
    {
        if (component != selectedComponent) return;

        selectedComponent = null;
    }
}
