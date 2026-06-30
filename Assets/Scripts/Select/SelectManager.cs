using System;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    private static SelectManager instance;
    public static SelectManager Instance => instance;

    public SelectComponent SelectedComponent { get; private set; }

    private bool isSubscribed = false;

    public event Action<SelectComponent> OnComponentSelected;
    public event Action<SelectComponent> OnComponentDeselected;

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

    private void HandleComponentSelected(SelectComponent component)
    {
        SetSelectedComponent(component);
        OnComponentSelected?.Invoke(SelectedComponent);
    }

    private void HandleComponentDeselected(SelectComponent component)
    {
        if (component != SelectedComponent) return;

        SetSelectedComponent(null);;
        OnComponentDeselected?.Invoke(component);
    }

    private void SetSelectedComponent(SelectComponent selected)
    {
        SelectedComponent = selected;
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        SelectComponent.OnComponentSelected += HandleComponentSelected;
        SelectComponent.OnComponentDeselected += HandleComponentDeselected;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        SelectComponent.OnComponentSelected -= HandleComponentSelected;
        SelectComponent.OnComponentDeselected -= HandleComponentDeselected;

        isSubscribed = false;
    }
}