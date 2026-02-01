using UnityEngine;

public class SelectManager
{
    private static SelectManager _instance;
    public static SelectManager Instance => _instance ??= new SelectManager();

    private SelectManager()
    {
        EventBus.onObjectSelected += OnObjectSelected;
        EventBus.onObjectDeselected += OnObjectDeselected;
    }

    public SelectComponent selectedComponent { get; private set; }

    private void OnObjectSelected(SelectComponent selectComponent)
    {
        selectedComponent = selectComponent;
    }

    private void OnObjectDeselected()
    {
        selectedComponent = null;
    }
}
