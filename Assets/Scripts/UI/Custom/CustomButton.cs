using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum CustomButtonState
{
    Idle,
    Hovered,
    Pressed,
    Selected,
    Disabled
}

[System.Serializable]
public class CustomSelectableStateEntry
{
    public Color bodyColor;
    public ColorHolder bodyColorHolder;
    public Color contentColor;
    public ColorHolder contentColorHolder;
    public float scale;
}

public class CustomButton : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] public Graphic targetGraphic;
    [SerializeField] public Graphic[] contentGraphics = null;
    [SerializeField] public RectTransform scaleRoot = null;

    [SerializeField] private bool isInteractable = true;
    public bool IsInteractable => isInteractable;

    [SerializeField] private bool isSelectable = false;
    public bool IsSelectable { get { return isSelectable; } set { isSelectable = value; } }

    [SerializeField] private bool isScalable = false;
    public bool IsScalable { get { return isScalable; } set { isScalable = value; } }

    [SerializeField] private bool deselectOnOutsideClick = false;
    [SerializeField] private bool cancelPressWhenMoving = false;

    [SerializeField] private SelectGroup selectGroup;
    [SerializeField] private float stateTransitionTime = 0.2f;

    private float stateTransitionAlpha = 1f;

    [Header("States")]
    [SerializeField] private CustomButtonState state;
    public CustomButtonState State => state;

    public bool IsIdle => state == CustomButtonState.Idle;
    public bool IsHovered => state == CustomButtonState.Hovered;
    public bool IsPressed => state == CustomButtonState.Pressed;
    public bool IsSelected => state == CustomButtonState.Selected;
    public bool IsEnabled => state != CustomButtonState.Disabled;
    public bool isAnimating { get; private set; } = false;

    [SerializeField]
    private CustomSelectableStateEntry idleState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(0.95f, 0.95f, 0.95f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1f,
    };
    [SerializeField]
    private CustomSelectableStateEntry hoveredState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(1f, 1f, 1f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1.02f,
    };
    [SerializeField]
    private CustomSelectableStateEntry pressedState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(0.75f, 0.75f, 0.75f, 0.75f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 0.98f,
    };
    [SerializeField]
    private CustomSelectableStateEntry selectedState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(1f, 1f, 1f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1.05f,
    };
    [SerializeField]
    private CustomSelectableStateEntry disabledState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(0.25f, 0.25f, 0.25f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 0.95f,
    };
    private CustomSelectableStateEntry CurrentStateEntry => IsIdle ? idleState : IsHovered ? hoveredState : IsPressed ? pressedState : IsSelected ? selectedState : disabledState;

    private Color targetBodyColor;
    private Color targetContentColor;
    public Color CurrentBodyColor { get { return targetGraphic != null ? targetGraphic.color : Color.black; } set { if (targetGraphic != null) targetGraphic.color = value; } }
    public Color CurrentContentColor { get { return targetGraphic != null ? targetGraphic.color : Color.black; } set { if (targetGraphic != null) targetGraphic.color = value; } }
    public Vector3 CurrentScale { get { return scaleRoot != null ? scaleRoot.localScale : Vector3.one; } set { if (scaleRoot != null) scaleRoot.localScale = value; } }

    private Vector3 pressedButtonPosition;

    public UnityEvent OnPressed = new();
    [FormerlySerializedAs("onReleased")] public UnityEvent OnReleased = new();
    public UnityEvent OnSelected = new();
    public UnityEvent OnDeselected = new();
    public UnityEvent OnHovered = new();
    public UnityEvent OnUnhovered = new();

    public event Action<CustomButtonState> OnStateChanged;

    public static event Action<CustomButton> OnButtonStateChanged;
    public static event Action<CustomButton> OnButtonPressed;
    public static event Action<CustomButton> OnButtonReleased;

    public event Action OnTutorialInteracted;

    protected override void Awake()
    {
        base.Awake();

        UpdateBodyTargetColor();
        UpdateContentTargetColor();
        UpdateColor();
        UpdateScale();
        UpdateSelectGroup();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        CustomUIManager.Instance.RegisterCustomButton(this);

        //InputListener.Instance.OnPressed += OnPointerPressed;
        InputListener.Instance.OnReleased += OnPointerReleased;

        EndTransitionAnimation();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CustomUIManager.Instance.UnregisterCustomButton(this);

        //InputListener.Instance.OnPressed -= OnPointerPressed;
        InputListener.Instance.OnReleased -= OnPointerReleased;

        if (state == CustomButtonState.Hovered) {
            SetState(CustomButtonState.Idle);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (selectGroup == null) return;

        selectGroup.RemoveButton(this);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        SetState(state);
        UpdateBodyTargetColor();
        UpdateContentTargetColor();
        SetStateTransitionAlpha(1f);
    }

    protected override void Reset()
    {
        base.Reset();

        if (targetGraphic == null) {
            var background = GetComponent<Graphic>();
            if (background == null) return;

            targetGraphic = background;
            scaleRoot = background.rectTransform;
        }
    }
#endif

    public void Tick()
    {
        if (!gameObject.activeInHierarchy) return;
        if (!enabled) return;

        if (isAnimating) {
            ApplyInteractionAlpha();
        }

        if (cancelPressWhenMoving && IsPressed && (pressedButtonPosition - transform.position).sqrMagnitude >= 1f) {
            SetState(CustomButtonState.Idle);
        }
    }

    // Enable
    private void Enable()
    {
    }

    private void Disable()
    {
        UpdateBodyTargetColor();
        UpdateContentTargetColor();
    }

    public void SetSelectGroup(SelectGroup selectGroup)
    {
        this.selectGroup = selectGroup;
        selectGroup.AddButton(this);
    }

    public void RemoveSelectGroup()
    {
        selectGroup = null;
        selectGroup.RemoveButton(this);
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }

    private void UpdateSelectGroup()
    {
        if (selectGroup != null) {
            selectGroup.AddButton(this);
        }
    }

    // Idle
    private void Idle()
    {
        UpdateBodyTargetColor();
        UpdateContentTargetColor();
    }

    // Hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsEnabled) return;
        if (!IsInteractable) return;
        if (IsSelected) return;

        SetState(CustomButtonState.Hovered);
    }

    private void Hover()
    {
        UpdateBodyTargetColor();
        UpdateContentTargetColor();
        OnHovered?.Invoke();
    }

    // Unhover
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsEnabled) return;
        if (!IsInteractable) return;
        if (IsSelected) return;

        SetState(CustomButtonState.Idle);
    }

    private void Unhover()
    {
        OnUnhovered?.Invoke();
    }

    // Press
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsEnabled) return;
        if (!IsInteractable) return;
        if (IsPressed) return;
        if (!IsHovered) return;

        SetState(CustomButtonState.Pressed);
    }

    private void Press()
    {
        UpdateBodyTargetColor();
        UpdateContentTargetColor();

        pressedButtonPosition = transform.position;

        OnPressed?.Invoke();
        OnButtonPressed?.Invoke(this);
    }

    // Release
    private void OnPointerReleased()
    {
        if (!IsEnabled) return;
        if (!IsInteractable) return;

        if (IsPressed) {
            if (IsSelectable)
                SetState(CustomButtonState.Selected);
            else
                SetState(CustomButtonState.Hovered);

            Release();
        }
        else if (IsIdle) {
            if (PointerUtils.GetRaycastUIResult().gameObject == gameObject) {
                SetState(CustomButtonState.Hovered);
            }
        }
        else if (!IsIdle) {
            var go = PointerUtils.GetRaycastUIResult().gameObject;
            var button = go != null ? go.GetComponent<CustomButton>() : null;

            if (deselectOnOutsideClick) {
                if (go == null || go != gameObject) {
                    SetState(CustomButtonState.Idle);
                }
            }
        }
    }

    private void Release()
    {
        if (!PointerUtils.IsUIHovered(gameObject)) return;

        OnReleased?.Invoke();
        OnButtonReleased?.Invoke(this);
        OnTutorialInteracted?.Invoke();
    }

    // Select
    private void Select()
    {
        UpdateBodyTargetColor();
        UpdateContentTargetColor();

        if (selectGroup != null) {
            selectGroup.OnButtonSelected(this);
        }

        OnSelected?.Invoke();
    }

    private void Deselect()
    {
        OnDeselected?.Invoke();
    }

    // Set State
    public void SetState(CustomButtonState state)
    {
        if (state == this.state) return;

        ExitState(this.state);
        this.state = state;
        EnterState(this.state);

        HandleStateChanged();
    }

    private void EnterState(CustomButtonState state)
    {
        switch (state) {
            case CustomButtonState.Idle:
                Idle();
                break;
            case CustomButtonState.Hovered:
                Hover();
                break;
            case CustomButtonState.Pressed:
                Press();
                break;
            case CustomButtonState.Selected:
                Select();
                break;
            case CustomButtonState.Disabled:
                Disable();
                break;
        }
    }

    private void ExitState(CustomButtonState state)
    {
        switch (state) {
            case CustomButtonState.Hovered:
                Unhover();
                break;
            case CustomButtonState.Selected:
                Deselect();
                break;
            case CustomButtonState.Disabled:
                Enable();
                break;
        }
    }

    private void HandleStateChanged()
    {
        if (gameObject.activeInHierarchy && gameObject.activeSelf) {
            ResetTransitionAnimation();
        }
        else {
            EndTransitionAnimation();
        }

        OnStateChanged?.Invoke(state);
        OnButtonStateChanged?.Invoke(this);
    }

    // Interaction
    public void EndTransitionAnimation()
    {
        SetStateTransitionAlpha(1f);
    }

    private void ApplyInteractionAlpha()
    {
        float duration = Mathf.Max(stateTransitionTime, 0.0001f);
        SetStateTransitionAlpha(stateTransitionAlpha + Time.deltaTime / duration);

        if (stateTransitionAlpha >= 1f)
            isAnimating = false;
    }

    private void ResetTransitionAnimation()
    {
        SetStateTransitionAlpha(0f);
    }

    private void SetStateTransitionAlpha(float value)
    {
        stateTransitionAlpha = value;
        stateTransitionAlpha = math.clamp(stateTransitionAlpha, 0, 1);

        if (stateTransitionAlpha >= 1) {
            isAnimating = false;
        }
        else {
            isAnimating = true;
        }

        UpdateColor();
        if (isScalable && scaleRoot != null) {
            UpdateScale();
        }
    }

    private void UpdateColor()
    {
        if (targetGraphic != null) {
            targetGraphic.color = Color.Lerp(targetGraphic.color, targetBodyColor, stateTransitionAlpha);
        }

        if (contentGraphics != null) {
            foreach (var graphic in contentGraphics) {
                if (graphic == null) continue;

                graphic.color = targetContentColor;
            }
        }
    }

    private void UpdateScale()
    {
        float targetScale = CurrentStateEntry.scale;
        CurrentScale = math.lerp(CurrentScale, new Vector3(targetScale, targetScale, targetScale), stateTransitionAlpha);
    }

    public void UpdateCurrentColorHolder()
    {
        if (IsEnabled && idleState.bodyColorHolder != null) {
            targetGraphic.color = idleState.bodyColorHolder.color;
            return;
        }

        if (!IsEnabled && disabledState.bodyColorHolder != null) {
            targetGraphic.color = disabledState.bodyColor;
            return;
        }
    }

    private void UpdateBodyTargetColor()
    {
        ColorHolder colorHolder = CurrentStateEntry.bodyColorHolder;
        Color color = colorHolder != null ? colorHolder.color : CurrentStateEntry.bodyColor;
        targetBodyColor = color;
    }

    private void UpdateContentTargetColor()
    {
        ColorHolder colorHolder = CurrentStateEntry.contentColorHolder;
        Color color = colorHolder != null ? colorHolder.color : CurrentStateEntry.contentColor;
        targetContentColor = color;
    }
}