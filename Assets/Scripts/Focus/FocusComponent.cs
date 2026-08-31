using System;
using UnityEngine;

public class FocusComponent : MonoBehaviour, IClickable
{
    [SerializeField] private FocusPointer focusPointerPrefab;
    public FocusPointer FocusPointerPrefab => focusPointerPrefab;

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    public bool IsFocused { get; private set; } = false;

    private bool isClickable = true;
    public bool IsClickable { get { return isClickable; } set { isClickable = value; } }

    public event Action<bool> OnFocusChanged;
    public static event Action<FocusComponent, bool> OnComponentFocusedChanged;

    public event Action OnClicked;
    public static event Action<FocusComponent> OnGlobalClicked;

    public static event Action<FocusComponent> OnComponentDestroyed;

    private void OnDestroy()
    {
        OnComponentDestroyed?.Invoke(this);
    }

    public void SetFocused(bool focused)
    {
        if (focused == IsFocused) return;

        IsFocused = focused;

        OnFocusChanged?.Invoke(focused);
        OnComponentFocusedChanged?.Invoke(this, focused);
    }

    public void Click()
    {
        SetFocused(!IsFocused);

        OnClicked?.Invoke();
        OnGlobalClicked?.Invoke(this);
    }

    public bool ShouldClick()
    {
        return IsClickable;
    }
}