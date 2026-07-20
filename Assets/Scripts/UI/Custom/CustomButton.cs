using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEngine.CullingGroup;

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

public class CustomButton : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    [SerializeField] private CustomSelectableStateEntry idleState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(0.95f, 0.95f, 0.95f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1f,
    };
    [SerializeField] private CustomSelectableStateEntry hoveredState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(1f, 1f, 1f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1.02f,
    };
    [SerializeField] private CustomSelectableStateEntry pressedState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(0.75f, 0.75f, 0.75f, 0.75f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 0.98f,
    };
    [SerializeField] private CustomSelectableStateEntry selectedState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(1f, 1f, 1f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1.05f,
    };
    [SerializeField] private CustomSelectableStateEntry disabledState = new CustomSelectableStateEntry()
    {
        bodyColor = new Color(0.25f, 0.25f, 0.25f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 0.95f,
    };
    private CustomSelectableStateEntry CurrentStateEntry => IsIdle ? idleState : IsHovered ? hoveredState : IsPressed ? pressedState : IsSelected ? selectedState : disabledState;

    private Color targetBodyColor;
    private Color targetContentColor;
    public Color CurrentBodyColor { get { return targetGraphic ? targetGraphic.color : Color.black; } set { if (targetGraphic) targetGraphic.color = value; } }
    public Color CurrentContentColor { get { return targetGraphic ? targetGraphic.color : Color.black; } set { if (targetGraphic) targetGraphic.color = value; } }
    public Vector3 CurrentScale { get { return scaleRoot ? scaleRoot.localScale : Vector3.one; } set { if (scaleRoot) scaleRoot.localScale = value; } }

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

        InputListener.Instance.OnPressed += OnPointerPressed;
        InputListener.Instance.OnReleased += OnPointerReleased;

        EndTransitionAnimation();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CustomUIManager.Instance.UnregisterCustomButton(this);

        InputListener.Instance.OnPressed -= OnPointerPressed;
        InputListener.Instance.OnReleased -= OnPointerReleased;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (!selectGroup) return;

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

        if (!targetGraphic) {
            var background = GetComponent<Graphic>();
            if (!background) return;

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
        if (selectGroup) {
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
    private void OnPointerPressed()
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
        //if (!IsPressed && !deselectOnOutsideClick) return;

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
            //if (isSelectable) return;

            var go = PointerUtils.GetRaycastUIResult().gameObject;
            var button = go ? go.GetComponent<CustomButton>() : null;

            if (deselectOnOutsideClick) {
                if (!go || go != gameObject) {
                    SetState(CustomButtonState.Idle);
                }
            }
            //else {

            //}

            //if (button && (!selectGroup || button.selectGroup == selectGroup) && !deselectOnOutsideClick)
            //    SetState(CustomButtonState.Idle);
            //else if (deselectOnOutsideClick && !button)
            //    SetState(CustomButtonState.Idle);
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

        if (selectGroup) {
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
        if (isScalable && scaleRoot) {
            UpdateScale();
        }
    }

    private void UpdateColor()
    {
        if (targetGraphic) {
            targetGraphic.color = Color.Lerp(targetGraphic.color, targetBodyColor, stateTransitionAlpha);
        }

        if (contentGraphics != null) {
            foreach (var graphic in contentGraphics) {
                if (!graphic) continue;

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
        if (IsEnabled && idleState.bodyColorHolder) {
            targetGraphic.color = idleState.bodyColorHolder.color;
            return;
        }

        if (!IsEnabled && disabledState.bodyColorHolder) {
            targetGraphic.color = disabledState.bodyColor;
            return;
        }
    }

    private void UpdateBodyTargetColor()
    {
        ColorHolder colorHolder = CurrentStateEntry.bodyColorHolder;
        Color color = colorHolder ? colorHolder.color : CurrentStateEntry.bodyColor;
        targetBodyColor = color;
    }

    private void UpdateContentTargetColor()
    {
        ColorHolder colorHolder = CurrentStateEntry.contentColorHolder;
        Color color = colorHolder ? colorHolder.color : CurrentStateEntry.contentColor;
        targetContentColor = color;
    }
}

//#if UNITY_EDITOR
//[CustomEditor(typeof(CustomSelectable))]
//[CanEditMultipleObjects]
//public class CustomSelectableEditor : Editor
//{
//    // Main Button
//    SerializedProperty backgroundGraphic;
//    SerializedProperty contentGraphicProp;
//    SerializedProperty isEnabled;
//    SerializedProperty isInteractable;
//    SerializedProperty isSelectable;
//    SerializedProperty isScalable;
//    SerializedProperty scaleRoot;
//    SerializedProperty stateTransitionTimeProp;
//    SerializedProperty selectableGroupIndexProp;
//    SerializedProperty deselectOnOutsideClickProp;

//    SerializedProperty idleState;
//    SerializedProperty hoveredStateProp;
//    SerializedProperty pressedState;
//    SerializedProperty selectedState;
//    SerializedProperty disabledState;

//    SerializedProperty idleColorHolderProp;
//    SerializedProperty disabledColorHolderProp;


//    SerializedProperty idleScaleProp;
//    SerializedProperty disabledScaleProp;

//    private bool showMain = true;
//    private bool showGraphic = true;

//    private void OnEnable()
//    {
//        // Main Button
//        backgroundGraphic = serializedObject.FindProperty("backgroundGraphic");
//        contentGraphicProp = serializedObject.FindProperty("contentGraphic");
//        isEnabled = serializedObject.FindProperty("isEnabled");
//        isInteractable = serializedObject.FindProperty("isInteractable");
//        isSelectable = serializedObject.FindProperty("isSelectable");
//        isScalable = serializedObject.FindProperty("isScalable");
//        scaleRoot = serializedObject.FindProperty("scaleRoot");
//        stateTransitionTimeProp = serializedObject.FindProperty("stateTransitionTime");
//        selectableGroupIndexProp = serializedObject.FindProperty("selectableGroupIndex");
//        deselectOnOutsideClickProp = serializedObject.FindProperty("deselectOnOutsideClick");

//        idleState = serializedObject.FindProperty("idleState");
//        hoveredStateProp = serializedObject.FindProperty("hoveredState");
//        pressedState = serializedObject.FindProperty("pressedState");
//        selectedState = serializedObject.FindProperty("selectedState");
//        disabledState = serializedObject.FindProperty("disabledState");

//        idleColorHolderProp = idleState.FindPropertyRelative("backgroundColorHolder");
//        disabledColorHolderProp = disabledState.FindPropertyRelative("backgroundColorHolder");

//        idleScaleProp = idleState.FindPropertyRelative("scale");
//        disabledScaleProp = disabledState.FindPropertyRelative("scale");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        EditorGUI.BeginChangeCheck();

//        var selectable = (CustomSelectable)target;
//        Color color;
//        float scale;

//        var contentGraphic = ((CustomSelectable)target).contentGraphic;
//        Color contentColor = Color.white;

//        showMain = EditorGUILayout.Foldout(showMain, "Main", true);
//        if (showMain) {
//            EditorGUILayout.PropertyField(backgroundGraphic);
//            EditorGUILayout.PropertyField(contentGraphicProp);
//            EditorGUILayout.PropertyField(scaleRoot);
//            EditorGUILayout.PropertyField(isEnabled);
//            EditorGUILayout.PropertyField(isInteractable);
//            EditorGUILayout.PropertyField(isSelectable);
//            EditorGUILayout.PropertyField(isScalable);
//            EditorGUILayout.PropertyField(stateTransitionTimeProp);
//            EditorGUILayout.PropertyField(selectableGroupIndexProp);
//            EditorGUILayout.PropertyField(deselectOnOutsideClickProp);
//        }

//        // Graphic
//        EditorGUILayout.Space();
//        showGraphic = EditorGUILayout.Foldout(showGraphic, "Graphic", true);
//        if (showGraphic) {
//            EditorGUILayout.PropertyField(idleState);
//            EditorGUILayout.PropertyField(hoveredStateProp);
//            EditorGUILayout.PropertyField(pressedState);
//            EditorGUILayout.PropertyField(selectedState);
//            EditorGUILayout.PropertyField(disabledState);
//        }

//        // Apply color
//        if (!isEnabled.boolValue) {
//            color = GetCurrentColor(disabledColorHolderProp, disabledState.FindPropertyRelative("backgroundColor"));
//            scale = disabledScaleProp.floatValue;
//        }
//        else {
//            color = GetCurrentColor(idleColorHolderProp, idleState.FindPropertyRelative("backgroundColor"));
//            scale = idleScaleProp.floatValue;
//        }

//        // Content
//        if (EditorGUI.EndChangeCheck()) {
//            Undo.RecordObject(selectable.backgroundGraphic, "Background Graphic Color");
//            selectable.CurrentBackgroundColor = color;
//            selectable.CurrentScale = new Vector3(scale, scale, scale);
//            EditorUtility.SetDirty(selectable);
//            if (contentGraphic) {
//                Undo.RecordObject(selectable.backgroundGraphic, "Content Graphic Color");
//                selectable.CurrentContentColor = contentColor;
//                EditorUtility.SetDirty(contentGraphic);
//            }
//        }

//        serializedObject.ApplyModifiedProperties();
//    }


//    private void DrawIndependentColor(string label, SerializedProperty holderProp, ref SerializedProperty colorProp)
//    {
//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

//        EditorGUILayout.PropertyField(holderProp);
//        EditorGUILayout.PropertyField(colorProp);
//        serializedObject.ApplyModifiedProperties();
//    }

//    private Color GetCurrentColor(SerializedProperty holderProp, SerializedProperty colorProp)
//    {
//        Color color;
//        ColorHolder holder = holderProp.objectReferenceValue as ColorHolder;
//        if (holder)
//            color = holder.color;
//        else
//            color = colorProp.colorValue;
//        return color;
//    }
//}
//#endif
