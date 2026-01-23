using UnityEngine;

[CreateAssetMenu(fileName = "UIColor", menuName = "UI/UIColor")]
public class ColorHolder : ScriptableObject
{
    public Color color = new Color();

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateCustomSelectableColor();
    }
#endif

    private void UpdateCustomSelectableColor()
    {
        CustomSelectable[] selectables = FindObjectsByType<CustomSelectable>(FindObjectsSortMode.None);
        foreach (var selectable in selectables) {
            selectable.UpdateCurrentColorHolder();
        }
    }
}