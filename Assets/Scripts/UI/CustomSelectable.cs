using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class CustomSelectable : Selectable, IPointerEnterHandler, IInputListenable
{
    [SerializeField] private bool isSelectable = false;
    [SerializeField] private bool isScalable = false;

    private bool isHighlighted = false;
    private bool isPressed = false;
    private bool isSelected = false;
    private bool IsSelected => UISystem.Instance.selectedSelectables.Contains(this);

    [SerializeField] private RectTransform scaleRoot = null;
    [SerializeField] public Graphic content = null;
    private SelectableGroup selectableGroup = null;

    public const float selectedButtonDownScaleValue = 0.95f;
    public const float selectedButtonUpScaleValue = 1.07f;
    public const float selectedButtonScaleSpeed = 20f;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.white;
    [SerializeField] private Color pressedColor = Color.white;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color disabledColor = Color.white;

    [SerializeField] private ColorHolder normalColorHolder = null;
    [SerializeField] private ColorHolder highlightedColorHolder = null;
    [SerializeField] private ColorHolder pressedColorHolder = null;
    [SerializeField] private ColorHolder selectedColorHolder = null;
    [SerializeField] private ColorHolder disabledColorHolder = null;

    // Content Color
    [SerializeField] private Color contentNormalColor = Color.white;
    [SerializeField] private Color contentSelectedColor = Color.white;

    [SerializeField] private ColorHolder contentNormalColorHolder = null;
    [SerializeField] private ColorHolder contentSelectedColorHolder = null;

    private Color targetColor;
    private Color targetContentColor;

    public event Action onRelease;

    protected override void OnEnable()
    {
        base.OnEnable();

        InputListener.Instance.OnPressed += OnPress;
        InputListener.Instance.OnReleased += OnRelease;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        InputListener.Instance.OnPressed -= OnPress;
        InputListener.Instance.OnReleased -= OnRelease;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        if (isScalable && scaleRoot)
            Scaling();
    }

    public void OnPress()
    {
        if (InputUtils.GetCurrentRaycastResult().gameObject == gameObject) {
            isPressed = true;
        }
    }

    public void OnRelease()
    {
        if (InputUtils.GetCurrentRaycastResult().gameObject == gameObject) {
            isPressed = true;
        }

        if (IsPressed)
        GameObject go = InputUtils.GetCurrentRaycastResult().gameObject;
        if (!go || go && go != gameObject) {
            Deselect();
        }

        if (InputUtils.GetCurrentRaycastResult().gameObject == gameObject)
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("down");
        base.OnPointerDown(eventData);

        Press();
    }

    private void Press()
    {
        isPressed = true;
        AssignColors();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("up");
        base.OnPointerUp(eventData);

        Release();
    }

    private void Release()
    {
        isPressed = false;
        if (isSelectable) {
            RaycastResult result = InputUtils.GetCurrentRaycastResult();
            if (result.gameObject == gameObject) {
                Select();
            }
            else {
                Deselect();
            }
        }
        AssignColors();
        onRelease?.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        isHighlighted = true;
        AssignColors();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        isHighlighted = false;
        AssignColors();
    }

    public override void Select()
    {
        base.Select();

        if (isSelectable)
            isSelected = true;
    }

    public void Deselect()
    {
        Debug.Log("Deselect");
        if (isSelectable)
            isSelected = false;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void Scaling()
    {
        if (isPressed)
            scaleRoot.localScale = math.lerp(scaleRoot.localScale, new Vector3(selectedButtonDownScaleValue, selectedButtonDownScaleValue, 1f), selectedButtonScaleSpeed * Time.deltaTime);
        else if (isSelected)
            scaleRoot.localScale = math.lerp(scaleRoot.localScale, new Vector3(selectedButtonUpScaleValue, selectedButtonUpScaleValue, 1f), selectedButtonScaleSpeed * Time.deltaTime);
        else
            scaleRoot.localScale = math.lerp(scaleRoot.localScale, Vector3.one, selectedButtonScaleSpeed * Time.deltaTime);
    }

    public void AssignColors()
    {
        if (!interactable) {
            if (disabledColorHolder)
                targetColor = disabledColorHolder.color;
            else
                targetColor = disabledColor;
        }
        else if (isPressed) {
            Debug.Log("isPressed");
            if (pressedColorHolder)
                targetColor = pressedColorHolder.color;
            else
                targetColor = pressedColor;
        }
        else if (isSelected) {
            if (selectedColorHolder)
                targetColor = selectedColorHolder.color;
            else
                targetColor = selectedColor;

            if (contentSelectedColorHolder)
                targetContentColor = contentSelectedColorHolder.color;
            else
                targetContentColor = contentSelectedColor;
        }
        else if (isHighlighted) {
            if (highlightedColorHolder)
                targetColor = highlightedColorHolder.color;
            else
                targetColor = highlightedColor;
        }
        else {
            if (normalColorHolder)
                targetColor = normalColorHolder.color;
            else
                targetColor = normalColor;

            if (contentNormalColorHolder)
                targetContentColor = contentNormalColorHolder.color;
            else
                targetContentColor = contentNormalColor;
        }

        ColorBlock block = colors;
        block.normalColor = targetColor;
        block.highlightedColor = targetColor;
        block.pressedColor = targetColor;
        block.selectedColor = targetColor;
        block.disabledColor = targetColor;
        colors = block;

        if (content) {
            content.color = targetContentColor;
        }
    }

    public void SetSelectableGroup(SelectableGroup group)
    {
        selectableGroup = group;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomSelectable))]
public class MainButtonEditor : SelectableEditor
{
    // Main Button
    SerializedProperty isSelectableProp;
    SerializedProperty isScalableProp;
    SerializedProperty scaleRootProp;
    SerializedProperty contentProp;

    SerializedProperty normalColorProp;
    SerializedProperty highlightedColorProp;
    SerializedProperty pressedColorProp;
    SerializedProperty selectedColorProp;
    SerializedProperty disabledColorProp;

    SerializedProperty normalColorHolderProp;
    SerializedProperty highlightedColorHolderProp;
    SerializedProperty pressedColorHolderProp;
    SerializedProperty selectedColorHolderProp;
    SerializedProperty disabledColorHolderProp;

    SerializedProperty contentNormalColorProp;
    SerializedProperty contentSelectedColorProp;

    SerializedProperty contentNormalColorHolderProp;
    SerializedProperty contentSelectedColorHolderProp;

    protected override void OnEnable()
    {
        // Main Button
        isSelectableProp = serializedObject.FindProperty("isSelectable");
        isScalableProp = serializedObject.FindProperty("isScalable");
        scaleRootProp = serializedObject.FindProperty("scaleRoot");
        contentProp = serializedObject.FindProperty("content");

        normalColorProp = serializedObject.FindProperty("normalColor");
        highlightedColorProp = serializedObject.FindProperty("highlightedColor");
        pressedColorProp = serializedObject.FindProperty("pressedColor");
        selectedColorProp = serializedObject.FindProperty("selectedColor");
        disabledColorProp = serializedObject.FindProperty("disabledColor");

        normalColorHolderProp = serializedObject.FindProperty("normalColorHolder");
        highlightedColorHolderProp = serializedObject.FindProperty("highlightedColorHolder");
        pressedColorHolderProp = serializedObject.FindProperty("pressedColorHolder");
        selectedColorHolderProp = serializedObject.FindProperty("selectedColorHolder");
        disabledColorHolderProp = serializedObject.FindProperty("disabledColorHolder");

        contentNormalColorProp = serializedObject.FindProperty("contentNormalColor");
        contentSelectedColorProp = serializedObject.FindProperty("contentSelectedColor");

        contentNormalColorHolderProp = serializedObject.FindProperty("contentNormalColorHolder");
        contentSelectedColorHolderProp = serializedObject.FindProperty("contentSelectedColorHolder");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Main Button", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isSelectableProp);
        EditorGUILayout.PropertyField(isScalableProp);
        EditorGUILayout.PropertyField(scaleRootProp);
        EditorGUILayout.PropertyField(contentProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Button Colors", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        
        var selectable = (Selectable)target;
        ColorBlock colors = selectable.colors;

        colors.normalColor = DrawIndependentColor("Normal Color", normalColorHolderProp, ref normalColorProp);
        colors.highlightedColor = DrawIndependentColor("Highlighted Color", highlightedColorHolderProp, ref highlightedColorProp);
        colors.pressedColor = DrawIndependentColor("Pressed Color", pressedColorHolderProp, ref pressedColorProp);
        colors.selectedColor = DrawIndependentColor("Selected Color", selectedColorHolderProp, ref selectedColorProp);
        colors.disabledColor = DrawIndependentColor("Disabled Color", disabledColorHolderProp, ref disabledColorProp);

        var graphic = ((CustomSelectable)target).content;
        Color contentColor = Color.white;
        if (graphic) {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Content Colors", EditorStyles.boldLabel);

            contentColor = DrawIndependentColor("Normal Color", contentNormalColorHolderProp, ref contentNormalColorProp);
            DrawIndependentColor("Selected Color", contentSelectedColorHolderProp, ref contentSelectedColorProp);
        }
        
        if (EditorGUI.EndChangeCheck()) {
            selectable.colors = colors;
            EditorUtility.SetDirty(selectable);
            if (graphic) {
                graphic.color = contentColor;
                EditorUtility.SetDirty(graphic);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }


    Color DrawIndependentColor(string label, SerializedProperty holderProp, ref SerializedProperty colorProp)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        Color color;
        EditorGUILayout.PropertyField(holderProp);
        EditorGUILayout.PropertyField(colorProp);
        serializedObject.ApplyModifiedProperties();
        ColorHolder holder = holderProp.objectReferenceValue as ColorHolder;

        if (holder) {
            color = holder.color;
        }
        else {
            color = colorProp.colorValue;
        }

        // Возвращаем текущее значение цвета для ColorBlock
        return holder ? holder.color : colorProp.colorValue;
    }
}
#endif
