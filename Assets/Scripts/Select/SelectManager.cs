using UnityEngine;

public class SelectManager : MonoBehaviour
{
    private static SelectManager _instance;
    public static SelectManager Instance => _instance ??= new SelectManager();

    private SelectComponent selectedComponent;

    private void OnEnable()
    {
        SelectComponent.onSelectedComponent += OnSelectedComponent;
        SelectComponent.onDeselectedComponent += OnDeselectedComponent;
    }

    private void OnDisable()
    {
        SelectComponent.onSelectedComponent -= OnSelectedComponent;
        SelectComponent.onDeselectedComponent -= OnDeselectedComponent;
    }

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