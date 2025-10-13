using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class MainButton : Button
{
    public RectTransform rectTransform = null;
    public bool isSelected { get; private set; } = false;
    public bool isPressed { get; private set; } = false;

    [SerializeField] private bool isSelectable = false;
    [SerializeField] private bool isScalable = false;
    [SerializeField] private bool isNeededCorrectPosition = false;
    [SerializeField] private Vector2 correctPositionDirection = Vector2.zero;

    [SerializeField] private GameObject content = null;

    private Vector2 initialSize = Vector2.zero;
    private Vector2 initialPosition = Vector2.zero;

    public const float selectedButtonDownScaleValue = 0.95f;
    public const float selectedButtonUpScaleValue = 1.07f;
    public const float selectedButtonScaleSpeed = 20f;

    [SerializeField] private UIColor normalColor = null;
    [SerializeField] private UIColor pressedColor = null;
    [SerializeField] private UIColor selectedColor = null;

    [SerializeField] private UIColor contentNormalColor = null;
    [SerializeField] private UIColor contentSelectedColor = null;

    private ColorBlock notSelectedColorBlock = new ColorBlock();
    private ColorBlock selectedColorBlock = new ColorBlock();

    public System.Action onPress;
    public System.Action onRelease;

    protected override void Awake()
    {
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        initialSize = rectTransform.sizeDelta;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        if (isScalable)
        {
            if (isPressed)
                rectTransform.localScale = math.lerp(rectTransform.localScale, new Vector3(selectedButtonDownScaleValue, selectedButtonDownScaleValue, 1f), selectedButtonScaleSpeed * Time.deltaTime);
            else if (isSelected)
                rectTransform.localScale = math.lerp(rectTransform.localScale, new Vector3(selectedButtonUpScaleValue, selectedButtonUpScaleValue, 1f), selectedButtonScaleSpeed * Time.deltaTime);
            else
                rectTransform.localScale = math.lerp(rectTransform.localScale, Vector3.one, selectedButtonScaleSpeed * Time.deltaTime);

            if (isNeededCorrectPosition)
            {
                rectTransform.anchoredPosition = initialPosition + (((initialSize * rectTransform.localScale - initialSize) / 2) * correctPositionDirection);
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (!isSelected)
        {
            isPressed = true;

            onPress?.Invoke();
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (isPressed)
        {
            isPressed = false;

            onRelease?.Invoke();
        }
    }

    public override void Select()
    {
        base.Select();

        isSelected = true;

        ApplyColors();

        colors = selectedColorBlock;

        if (content)
        {
            TextMeshProUGUI textContent = content.GetComponent<TextMeshProUGUI>();
            Image imageContent = content.GetComponent<Image>();

            if (textContent)
            {
                textContent.color = contentSelectedColor.color;
            }
            if (imageContent)
            {
                imageContent.color = contentSelectedColor.color;
            }
        }
    }

    public void Deselect()
    {
        isSelected = false;

        ApplyColors();

        colors = notSelectedColorBlock;

        if (content)
        {
            TextMeshProUGUI textContent = content.GetComponent<TextMeshProUGUI>();
            Image imageContent = content.GetComponent<Image>();

            if (textContent)
            {
                textContent.color = contentNormalColor.color;
            }
            if (imageContent)
            {
                imageContent.color = contentNormalColor.color;
            }
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
    }

    public void ApplyColors()
    {
        selectedColorBlock = new ColorBlock();
        notSelectedColorBlock = new ColorBlock();

        if (normalColor)
        {
            notSelectedColorBlock.normalColor = normalColor.color;
            notSelectedColorBlock.selectedColor = normalColor.color;
            notSelectedColorBlock.highlightedColor = normalColor.color;
            selectedColorBlock.highlightedColor = normalColor.color;
        }

        if (pressedColor)
        {
            notSelectedColorBlock.pressedColor = pressedColor.color;
        }

        if (selectedColor)
        {
            selectedColorBlock.normalColor = selectedColor.color;
            selectedColorBlock.pressedColor = selectedColor.color;
            selectedColorBlock.selectedColor = selectedColor.color;
        }

        selectedColorBlock.colorMultiplier = 1f;
        notSelectedColorBlock.colorMultiplier = 1f;

        selectedColorBlock.fadeDuration = 0.1f;
        notSelectedColorBlock.fadeDuration = 0.1f;

        if (isSelected)
            colors = selectedColorBlock;
        else
            colors = notSelectedColorBlock;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MainButton))]
public class MainButtonEditor : ButtonEditor
{
    SerializedProperty isSelectableProp;
    SerializedProperty isScalableProp;
    SerializedProperty isNeededCorrectPositionProp;
    SerializedProperty correctPositionDirectionProp;
    SerializedProperty contentProp;

    SerializedProperty normalColorProp;
    SerializedProperty pressedColorProp;
    SerializedProperty selectedColorProp;

    SerializedProperty contentNormalColorProp;
    SerializedProperty contentSelectedColorProp;

    protected override void OnEnable()
    {
        base.OnEnable();

        isSelectableProp = serializedObject.FindProperty("isSelectable");
        isScalableProp = serializedObject.FindProperty("isScalable");
        isNeededCorrectPositionProp = serializedObject.FindProperty("isNeededCorrectPosition");
        correctPositionDirectionProp = serializedObject.FindProperty("correctPositionDirection");
        contentProp = serializedObject.FindProperty("content");

        normalColorProp = serializedObject.FindProperty("normalColor");
        pressedColorProp = serializedObject.FindProperty("pressedColor");
        selectedColorProp = serializedObject.FindProperty("selectedColor");

        contentNormalColorProp = serializedObject.FindProperty("contentNormalColor");
        contentSelectedColorProp = serializedObject.FindProperty("contentSelectedColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Main Button", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(isSelectableProp);
        EditorGUILayout.PropertyField(isScalableProp);
        EditorGUILayout.PropertyField(isNeededCorrectPositionProp);
        EditorGUILayout.PropertyField(correctPositionDirectionProp);
        EditorGUILayout.PropertyField(contentProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Button Colors", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(normalColorProp);
        EditorGUILayout.PropertyField(pressedColorProp);
        EditorGUILayout.PropertyField(selectedColorProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Content Colors", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(contentNormalColorProp);
        EditorGUILayout.PropertyField(contentSelectedColorProp);

        MainButton mainButton = (MainButton)target;
        if (GUILayout.Button("Apply Colors"))
        {
            mainButton.ApplyColors();
            EditorUtility.SetDirty(mainButton);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
