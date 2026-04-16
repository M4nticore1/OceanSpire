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
    public static event Action<SelectComponent> onComponentSelected;
    public static event Action<SelectComponent> onComponentDeselected;

    private void OnEnable()
    {
        EventBus.onPlayerClicked += OnPlayerClicked;
    }

    private void OnDisable()
    {
        EventBus.onPlayerClicked -= OnPlayerClicked;
    }

    public void Select()
    {
        isSelected = true;
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
        onComponentSelected?.Invoke(this);
    }

    public void Deselect()
    {
        isSelected = false;

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
        onComponentDeselected?.Invoke(this);
    }

    // IClickable
    public void Click()
    {
        if (isSelected) {
            Deselect();
        }
        else {
            Select();
        }
    }

    public bool CanClick()
    {
        return isClickable;
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }

    private void OnPlayerClicked(GameObject clicked)
    {
        if (!isClickable) return;

        if (clicked != gameObject && isSelected) {
            Deselect();
        }
    }
}