using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectComponent : MonoBehaviour, IClickable
{
    public bool isSelected { get; private set; } = false;
    private Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();

    [SerializeField] private bool isClickable = true;
    public bool IsClickable => isClickable;

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
        if (!isClickable) return;

        if (clicked != gameObject && isSelected) {
            SetSelected(false);
        }
    }

    public void SetSelected(bool value)
    {
        if (value == isSelected) return;

        isSelected = value;
        if (isSelected) {
            OnSelected();
        }
        else {
            OnDeselected();
        }
    }

    private void OnSelected()
    {
        layers.Clear();

        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            if (child.GetComponent<ParticleSystem>()) continue;

            layers.Add(child, child.layer);
            child.layer = LayerMask.NameToLayer("Outlined");
        }

        onSelected?.Invoke();
        EventBus.InvokeSelectedObject(this);
    }

    private void OnDeselected()
    {
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            if (!layers.ContainsKey(child)) continue;

            child.layer = layers[child];
        }

        onDeselected?.Invoke();
        EventBus.InvokeDeselectedObject(this);
    }

    // IClickable
    public void Click()
    {
        SetSelected(!isSelected);
    }

    public bool CanClick()
    {
        return isClickable;
    }

    //private void OnSelectedComponent(SelectComponent component)
    //{
    //    if (!isSelected) return;
    //    if (component == this) return;

    //    Deselect();
    //}
}
