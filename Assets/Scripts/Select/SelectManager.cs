using System;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    private static SelectManager instance;
    public static SelectManager Instance => instance;

    public SelectComponent SelectedComponent { get; private set; }

    private bool isSubscribed = false;

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
        TrySubscribe();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    public void Deselect()
    {
        SelectedComponent.Deselect();
    }

    public Building GetSelectedBuilding()
    {
        if (!SelectedComponent) return null;

        Building building = SelectedComponent.GetComponent<Building>();
        return building;
    }

    public Human GetSelectedHuman()
    {
        if (!SelectedComponent) return null;

        return SelectedComponent.GetComponent<Human>();
    }

    private void OnComponentSelected(SelectComponent component)
    {
        SetSelectedComponent(component);
        onComponentSelected?.Invoke(SelectedComponent);
    }

    private void OnComponentDeselected(SelectComponent component)
    {
        if (component != SelectedComponent) return;

        SetSelectedComponent(null);;
        onComponentDeselected?.Invoke(component);
    }

    private void SetSelectedComponent(SelectComponent selected)
    {
        SelectedComponent = selected;
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        SelectComponent.OnComponentSelected += OnComponentSelected;
        SelectComponent.OnComponentDeselected += OnComponentDeselected;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        SelectComponent.OnComponentSelected -= OnComponentSelected;
        SelectComponent.OnComponentDeselected -= OnComponentDeselected;

        isSubscribed = false;
    }
}