using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NUnit.Framework.Constraints;
using NUnit.Framework;
using Newtonsoft.Json.Bson;
using UnityEngine.Serialization;





#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public enum CustomSelectableState
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
    public Color backgroundColor;
    public ColorHolder backgroundColorHolder;
    public Color contentColor;
    public ColorHolder contentColorHolder;
    public float scale;
}

[RequireComponent(typeof(Image))]
public class CustomSelectable : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IInputListenable
{
    [SerializeField] public Graphic targetGraphic;
    [SerializeField] public Graphic contentGraphic = null;
    [SerializeField] public RectTransform scaleRoot = null;

    [SerializeField] private bool isEnabled = true;
    [SerializeField] private bool isInteractable = true;
    public bool IsInteractable { get { return isInteractable; } set { isInteractable = value; } }
    [SerializeField] private bool isSelectable = false;
    public bool IsSelectable { get { return isSelectable; } set { isSelectable = value; } }
    [SerializeField] private bool isScalable = false;
    public bool IsScalable { get { return isScalable; } set { isScalable = value; } }
    [SerializeField] private bool deselectOnOutsideClick = true;

    [SerializeField] private int selectableGroupIndex = -1;
    [SerializeField] private float stateTransitionTime = 0.3f;
    private float stateTransitionAlpha = 0f;

    private CustomSelectableState state = CustomSelectableState.Idle;

    public bool IsHovered => state == CustomSelectableState.Hovered;
    public bool IsPressed => state == CustomSelectableState.Pressed;
    public bool IsSelected => state == CustomSelectableState.Selected;
    public bool isAnimating { get; private set; } = false;

    [SerializeField] private CustomSelectableStateEntry idleState = new CustomSelectableStateEntry()
    {
        backgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1f,
    };
    [SerializeField] private CustomSelectableStateEntry hoveredState = new CustomSelectableStateEntry()
    {
        backgroundColor = new Color(1f, 1f, 1f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1.02f,
    };
    [SerializeField] private CustomSelectableStateEntry pressedState = new CustomSelectableStateEntry()
    {
        backgroundColor = new Color(0.75f, 0.75f, 0.75f, 0.75f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 0.98f,
    };
    [SerializeField] private CustomSelectableStateEntry selectedState = new CustomSelectableStateEntry()
    {
        backgroundColor = new Color(1f, 1f, 1f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 1.05f,
    };
    [SerializeField] private CustomSelectableStateEntry disabledState = new CustomSelectableStateEntry()
    {
        backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f),
        contentColor = new Color(1f, 1f, 1f, 1f),
        scale = 0.95f,
    };
    CustomSelectableStateEntry currentState;

    private Color targetContentColor;
    public Color CurrentBackgroundColor { get { return targetGraphic ? targetGraphic.color : Color.black; } set { if (targetGraphic) targetGraphic.color = value; } }
    public Color CurrentContentColor { get { return targetGraphic ? targetGraphic.color : Color.black; } set { if (targetGraphic) targetGraphic.color = value; } }
    public Vector3 CurrentScale { get { return scaleRoot ? scaleRoot.localScale : Vector3.one; } set { if (scaleRoot) scaleRoot.localScale = value; } }

    public event Action onPressed;
    public event Action onReleased;
    public event Action onSelected;
    public event Action onDeselected;
    public event Action onHovered;
    public event Action onUnhovered;

    protected override void Awake()
    {
        base.Awake();

        SetState(CustomSelectableState.Idle);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        InputListener.Instance.onPressed += OnPress;
        InputListener.Instance.onReleased += OnRelease;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (InputListener.Instance) {
            InputListener.Instance.onPressed -= OnPress;
            InputListener.Instance.onReleased -= OnRelease;
        }
    }

    private void Update()
    {
        if (isAnimating) {
            ApplyInteractionAlpha();
            ApplyColor();
            if (isScalable && scaleRoot)
                ApplyScale();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (isEnabled) {
            if (targetGraphic) {
                if (idleState.backgroundColorHolder)
                    targetGraphic.color = idleState.backgroundColorHolder.color;
                else
                    targetGraphic.color = idleState.backgroundColor;
            }

            if (contentGraphic) {
                if (idleState.backgroundColorHolder)
                    contentGraphic.color = idleState.contentColorHolder.color;
                else
                    contentGraphic.color = idleState.contentColor;
            }
        }
        else {
            if (targetGraphic) {
                if (disabledState.backgroundColorHolder)
                    targetGraphic.color = disabledState.backgroundColorHolder.color;
                else
                    targetGraphic.color = disabledState.backgroundColor;
            }

            if (contentGraphic) {
                if (disabledState.backgroundColorHolder)
                    contentGraphic.color = disabledState.contentColorHolder.color;
                else
                    contentGraphic.color = disabledState.contentColor;
            }
        }
    }

    protected override void Reset()
    {
        base.Reset();

        if (!targetGraphic) {
            Graphic background = GetComponent<Graphic>();
            targetGraphic = background;
            scaleRoot = background.rectTransform;
        }
        if (!contentGraphic) {
            Graphic content = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Graphic>() : null;
            contentGraphic = content;
        }
    }
#endif

    // Enable
    private void Enable()
    {
        isEnabled = true;
    }

    private void Disable()
    {
        Debug.Log("Disable");
        isEnabled = false;
        currentState = disabledState;
    }

    // Idle
    private void Idle()
    {
        currentState = idleState;
    }

    // Hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEnabled) return;
        if (!IsInteractable) return;

        if (!IsSelected)
            SetState(CustomSelectableState.Hovered);
    }

    protected void Hover()
    {
        currentState = hoveredState;
        onHovered?.Invoke();
    }

    // Unhover
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEnabled) return;
        if (!IsInteractable) return;

        if (!IsSelected)
            SetState(CustomSelectableState.Idle);
    }

    protected void Unhover()
    {
        onUnhovered?.Invoke();
    }

    // Press
    public void OnPress()
    {
        if (!isEnabled) return;
        if (!IsInteractable) return;
        if (!IsHovered) return;

        SetState(CustomSelectableState.Pressed);
    }

    protected void Press()
    {
        currentState = pressedState;
        onPressed?.Invoke();
    }

    // Release
    public void OnRelease()
    {
        if (!isEnabled) return;
        if (!IsInteractable) return;

        Debug.Log("OnRelease");
        if (IsPressed) {
            Debug.Log("IsPressed");
            if (IsSelectable) {
                SetState(CustomSelectableState.Selected);
                return;
            }

            SetState(CustomSelectableState.Hovered);
            return;
        }

        GameObject go = PointerUtils.GetCurrentRaycastResult().gameObject;
        CustomSelectable selectable = go ? go.GetComponent<CustomSelectable>() : null;
        if (selectable && (selectable.selectableGroupIndex == selectableGroupIndex || selectableGroupIndex < 0)) {
            SetState(CustomSelectableState.Idle);
            return;
        }

        if (deselectOnOutsideClick && !selectable) {
            SetState(CustomSelectableState.Idle);
            return;
        }
    }

    private void Release()
    {
        Debug.Log("Release1");
        if (!PointerUtils.IsGameObjectHovered(gameObject)) return;

        Debug.Log("Release2");

        onReleased?.Invoke();
    }

    // Select
    public void Select()
    {
        currentState = selectedState;
        onSelected?.Invoke();
    }

    // Deselect
    public void Deselect()
    {
        onDeselected?.Invoke();
    }

    public void SetState(CustomSelectableState newState)
    {
        Debug.Log(state);
        if (newState == state) return;
        if (!IsInteractable) return;
        Debug.Log(state);

        ExitState(state);
        state = newState;
        EnterState(state);
        OnStateChange();
    }

    private void ExitState(CustomSelectableState state)
    {
        switch (state) {
            case CustomSelectableState.Hovered:
                Unhover();
                break;
            case CustomSelectableState.Pressed:
                Release();
                break;
            case CustomSelectableState.Selected:
                Deselect();
                break;
            case CustomSelectableState.Disabled:
                Enable();
                break;
        }
    }

    private void EnterState(CustomSelectableState state)
    {
        switch (state) {
            case CustomSelectableState.Idle:
                Idle();
                break;
            case CustomSelectableState.Hovered:
                Hover();
                break;
            case CustomSelectableState.Pressed:
                Press();
                break;
            case CustomSelectableState.Selected:
                Select();
                break;
            case CustomSelectableState.Disabled:
                Disable();
                break;
        }
    }

    private void OnStateChange()
    {
        ResetInteractionAlpha();
    }

    private void ApplyInteractionAlpha()
    {
        float duration = Mathf.Max(stateTransitionTime, 0.0001f);
        stateTransitionAlpha += Time.deltaTime / duration;
        if (stateTransitionAlpha >= 1f)         
            isAnimating = false;
    }

    private void ResetInteractionAlpha()
    {
        stateTransitionAlpha = 0f;
        isAnimating = true;
    }

    private void ApplyColor()
    {
        CurrentBackgroundColor = Color.Lerp(CurrentBackgroundColor, currentState.backgroundColor, stateTransitionAlpha);
        if (contentGraphic) {
            contentGraphic.color = targetContentColor;
        }
    }

    private void ApplyScale()
    {
        CurrentScale = math.lerp(CurrentScale, currentState.scale, stateTransitionAlpha);
    }

    public void UpdateCurrentColorHolder()
    {
        if (isEnabled && idleState.backgroundColorHolder) {
            targetGraphic.color = idleState.backgroundColorHolder.color;
            return;
        }

        if (!isEnabled && disabledState.backgroundColorHolder) {
            targetGraphic.color = disabledState.backgroundColor;
            return;
        }
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
