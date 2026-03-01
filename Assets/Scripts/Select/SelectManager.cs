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

    public SelectComponent selectedComponent { get; private set; }

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
