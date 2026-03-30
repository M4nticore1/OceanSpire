using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectComponent : MonoBehaviour, IClickable
{
    public bool isSelected { get; private set; } = false;
    private Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();

    private bool isClickable = true;

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

        List<GameObject> objects = GameUtils.GetAllChildren(transform);
        objects.Add(gameObject);

        foreach (GameObject child in objects) {
            if (child.GetComponent<UIBehaviour>()) continue;
            if (child.GetComponent<ParticleSystem>()) continue;

            layers.Add(child, child.layer);
            child.layer = LayerMask.NameToLayer("Outlined");
        }

        onSelected?.Invoke();
        EventBus.InvokeSelectedObject(this);
    }

    private void OnDeselected()
    {
        List<GameObject> objects = GameUtils.GetAllChildren(transform);
        objects.Add(gameObject);

        foreach (GameObject child in objects) {
            if (child.GetComponent<UIBehaviour>()) continue;

            if (layers.ContainsKey(child)) {
                child.layer = layers[child];
            }
            else {
                child.layer = LayerMask.NameToLayer("Default");
            }
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

    public void SetClickable(bool value)
    {
        isClickable = value;
    }
}