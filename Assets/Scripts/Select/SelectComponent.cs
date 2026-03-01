using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectComponent : MonoBehaviour
{
    public bool isSelected { get; private set; } = false;
    private Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();

    public event Action onSelected;
    public event Action onDeselected;

    private void OnEnable()
    {
        EventBus.onPlayerClicked += OnClicked;
    }

    private void OnDisable()
    {
        EventBus.onPlayerClicked -= OnClicked;
    }

    private void OnClicked(GameObject clicked)
    {
        if (clicked == gameObject && !isSelected) {
            Select();
        }
        else if (isSelected) {
            Deselect();
        }
    }

    private void Select()
    {
        layers.Clear();

        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            layers.Add(child, child.layer);
            child.layer = LayerMask.NameToLayer("Outlined");
        }

        isSelected = true;
        onSelected?.Invoke();
        EventBus.InvokeSelectedObject(this);
    }

    private void Deselect()
    {
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            if (!layers.ContainsKey(child)) continue;

            child.layer = layers[child];
        }

        isSelected = false;
        onDeselected?.Invoke();
        EventBus.InvokeDeselectedObject(this);
    }

    //private void OnSelectedComponent(SelectComponent component)
    //{
    //    if (!isSelected) return;
    //    if (component == this) return;

    //    Deselect();
    //}
}
