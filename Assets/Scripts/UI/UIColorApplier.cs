using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class UIColorGroup
{
    public ColorHolder uiColor;

    public Image[] images;
    public Button[] buttons;
    public Text[] texts;
}

#if UNITY_EDITOR
[ExecuteAlways]
[DisallowMultipleComponent]
public class UIColorApplier : MonoBehaviour
{
    public UIColorGroup[] colorGroups = new UIColorGroup[0];

    public void ApplyColor()
    {
        if (!Application.isPlaying)
        {
            for (int i = 0; i < colorGroups.Length; i++)
            {
                for (int j = 0; j < colorGroups[i].images.Length; j++)
                {
                    if (colorGroups[i].images[j])
                    {
                        Undo.RecordObject(colorGroups[i].images[j], "Apply Color");
                        colorGroups[i].images[j].color = colorGroups[i].uiColor.color;
                        EditorUtility.SetDirty(colorGroups[i].images[j]);
                    }
                }

                for (int j = 0; j < colorGroups[i].texts.Length; j++)
                {
                    if (colorGroups[i].texts[j])
                    {
                        Undo.RecordObject(colorGroups[i].texts[j], "Apply Color");
                        colorGroups[i].texts[j].color = colorGroups[i].uiColor.color;
                        EditorUtility.SetDirty(colorGroups[i].texts[j]);
                    }
                }

                for (int j = 0; j < colorGroups[i].buttons.Length; j++)
                {
                    if (colorGroups[i].buttons[j])
                    {
                        ColorBlock colorBlock = colorGroups[i].buttons[j].colors;
                        colorBlock.normalColor = colorGroups[i].uiColor.color;

                        Undo.RecordObject(colorGroups[i].buttons[j], "Apply Color");
                        colorGroups[i].buttons[j].colors = colorBlock;
                        EditorUtility.SetDirty(colorGroups[i].buttons[j]);
                    }
                }
            }
        }
    }
}
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(UIColorApplier))]
public class UIColorApplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIColorApplier applier = (UIColorApplier)target;
        if (GUILayout.Button("Apply Color"))
        {
            applier.ApplyColor();
        }
    }
}
#endif