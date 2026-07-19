using System;
using UnityEngine;

public class FocusComponent : MonoBehaviour, IClickable
{
    [SerializeField] private FocusPointer focusPointerPrefab;
    public FocusPointer FocusPointerPrefab => focusPointerPrefab;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    public bool IsFocused { get; private set; } = false;
    public bool IsClickable { get; private set; } = true;

    public static event Action<FocusComponent> OnFocusedChanged;
    public static event Action<FocusComponent> OnComponentDestroyed;

    public event Action OnClicked;
    public static event Action<FocusComponent> OnGlobalClicked;

    private void OnDestroy()
    {
        OnComponentDestroyed?.Invoke(this);
    }

    public void SetFocused(bool value)
    {
        if (value == IsFocused) return;

        IsFocused = value;
        OnFocusedChanged?.Invoke(this);
    }

    public void Click()
    {
        SetFocused(!IsFocused);

        OnClicked?.Invoke();
        OnGlobalClicked?.Invoke(this);
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
    }

    public bool ShouldClick()
    {
        return IsClickable;
    }
}