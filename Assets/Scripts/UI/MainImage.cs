using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class MainImage : Image
{
    [SerializeField] private ColorHolder mainColor = null;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        ApplyColor();
    }

    public void ApplyColor()
    {
        if (mainColor)
            color = mainColor.color;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MainImage))]
public class MainImageEditor : ImageEditor
{
    SerializedProperty mainColorProp;

    protected override void OnEnable()
    {
        base.OnEnable();

        mainColorProp = serializedObject.FindProperty("mainColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Main Image", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(mainColorProp);

        MainImage mainimage = (MainImage)target;
        if (GUILayout.Button("Apply Colors"))
        {
            mainimage.ApplyColor();
            EditorUtility.SetDirty(mainimage);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
