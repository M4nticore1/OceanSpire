using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class SelectComponent : MonoBehaviour, IClickable
{
    [field: SerializeField] public bool IsSelected { get; private set; } = false;
    private Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();

    [SerializeField] private bool isClickable = true;
    public bool IsClickable => isClickable;

    public event Action OnSelected;
    public event Action OnDeselected;

    public event Action OnClicked;

    public static event Action<SelectComponent> OnComponentSelected;
    public static event Action<SelectComponent> OnComponentDeselected;
    public static event Action<SelectComponent> OnComponentDestroyed;

    private void OnEnable()
    {
        EventBus.OnPlayerClicked += OnPlayerClicked;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerClicked -= OnPlayerClicked;

        OnComponentDestroyed?.Invoke(this);
    }

    public void TrySelect()
    {
        if (IsSelected) return;

        Select();
    }

    public void Select()
    {
        layers.Clear();

        var objects = GameUtils.GetAllChildren(transform);
        objects.Add(gameObject);

        foreach (GameObject child in objects) {
            if (!ShouldInteract(child)) continue;

            layers.Add(child, child.layer);
            child.layer = LayerMask.NameToLayer("Outlined");
        }

        IsSelected = true;
        OnSelected?.Invoke();
        OnComponentSelected?.Invoke(this);
    }

    public void TryDeselect()
    {
        if (!IsSelected) return;

        Deselect();
    }

    public void Deselect()
    {
        var objects = GameUtils.GetAllChildren(transform);
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
        OnDeselected?.Invoke();
        OnComponentDeselected?.Invoke(this);
    }

    // IClickable
    public void TryClick()
    {
        if (!ShouldClick()) return;

        Click();
    }

    public void Click()
    {
        if (IsSelected) {
            Deselect();
        }
        else {
            Select();
        }

        OnClicked?.Invoke();
    }

    public bool ShouldClick()
    {
        return isClickable;
    }

    public void SetClickable(bool value)
    {
        Debug.Log($"SelectSetCliacable {value}");
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