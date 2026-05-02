using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectComponent : MonoBehaviour, IClickable
{
    public bool IsSelected { get; private set; } = false;
    private Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();

    [SerializeField] private bool isClickable = true;

    public event Action onSelected;
    public event Action onDeselected;

    public static event Action<SelectComponent> onComponentSelected;
    public static event Action<SelectComponent> onComponentDeselected;
    public static event Action<SelectComponent> onComponentDestroyed;

    private void OnEnable()
    {
        EventBus.onPlayerClicked += OnPlayerClicked;
    }

    private void OnDisable()
    {
        EventBus.onPlayerClicked -= OnPlayerClicked;

        onComponentDestroyed?.Invoke(this);
    }

    public void TrySelect()
    {
        if (IsSelected) return;

        Select();
    }

    public void Select()
    {
        layers.Clear();

        List<GameObject> objects = GameUtils.GetAllChildren(transform);
        objects.Add(gameObject);

        foreach (GameObject child in objects) {
            if (!ShouldInteract(child)) continue;

            layers.Add(child, child.layer);
            child.layer = LayerMask.NameToLayer("Outlined");
        }

        IsSelected = true;
        onSelected?.Invoke();
        onComponentSelected?.Invoke(this);
    }

    public void TryDeselect()
    {
        if (!IsSelected) return;

        Deselect();
    }

    public void Deselect()
    {
        List<GameObject> objects = GameUtils.GetAllChildren(transform);
        objects.Add(gameObject);

        foreach (GameObject child in objects) {
            if (!ShouldInteract(child)) continue;

            if (layers.ContainsKey(child)) {
                child.layer = layers[child];
            }
            else {
                child.layer = LayerMask.NameToLayer("Default");
            }
        }

        IsSelected = false;
        onDeselected?.Invoke();
        onComponentDeselected?.Invoke(this);
    }

    // IClickable
    public void Click()
    {
        if (IsSelected) {
            Deselect();
        }
        else {
            Select();
        }
    }

    public bool ShouldClick()
    {
        return isClickable;
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }

    private void OnPlayerClicked(GameObject clicked)
    {
        if (clicked == gameObject) return;
        if (GameUtils.GetAllChildren(transform).Contains(clicked)) return;

        TryDeselect();
    }

    private bool ShouldInteract(GameObject transform)
    {
        if (transform.GetComponent<UIBehaviour>()) return false;
        if (transform.GetComponent<ParticleSystem>()) return false;
        if (transform.GetComponent<IgnoreSelect>()) return false;

        return true;
    }
}