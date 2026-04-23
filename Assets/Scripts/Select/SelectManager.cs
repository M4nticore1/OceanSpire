using System;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    private static SelectManager instance;
    public static SelectManager Instance => instance;

    public SelectComponent selectedComponent { get; private set; }

    public static event Action<SelectComponent> onComponentSelected;
    public static event Action<SelectComponent> onComponentDeselected;

    private void Awake()
    {
        if (instance) {
            Debug.Log("There is an extra SelectManager on the scene!");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        SelectComponent.onComponentSelected += OnComponentSelected;
        SelectComponent.onComponentDeselected += OnComponentDeselected;
    }

    private void OnDisable()
    {
        SelectComponent.onComponentSelected -= OnComponentSelected;
        SelectComponent.onComponentDeselected -= OnComponentDeselected;
    }

    public void Deselect()
    {
        selectedComponent.Deselect();
    }

    public Building GetSelectedBuilding()
    {
        if (!selectedComponent) return null;

        Building building = selectedComponent.GetComponent<Building>();
        return building;
    }

    public Human GetSelectedHuman()
    {
        if (!selectedComponent) return null;

        return selectedComponent.GetComponent<Human>();
    }

    private void OnComponentSelected(SelectComponent component)
    {
        SetSelectedComponent(component);
        onComponentSelected?.Invoke(selectedComponent);
    }

    private void OnComponentDeselected(SelectComponent component)
    {
        if (component != selectedComponent) return;

        SetSelectedComponent(null);;
        onComponentDeselected?.Invoke(component);
    }

    private void SetSelectedComponent(SelectComponent selected)
    {
        selectedComponent = selected;
    }
}