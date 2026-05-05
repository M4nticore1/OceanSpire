using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class CustomImage : Image
{
    [SerializeField] private ColorHolder colorHolder = null;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        ApplyColor();
    }

    protected override void Reset()
    {
        base.Reset();

        ApplyColor();
    }

    private void ApplyColor()
    {
        if (colorHolder == null) return;

        color = colorHolder.color;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomImage))]
public class MainImageEditor : ImageEditor
{
    SerializedProperty colorHolderProp;

    protected override void OnEnable()
    {
        base.OnEnable();

        colorHolderProp = serializedObject.FindProperty("colorHolder");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Custom Image", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(colorHolderProp);

        //MainImage mainimage = (MainImage)target;
        //if (GUILayout.Button("Apply Colors")) {
        //    mainimage.ApplyColor();
        //    EditorUtility.SetDirty(mainimage);
        //}

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
