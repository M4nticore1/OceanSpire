using UnityEngine;

public static class SkillDescriptionWidgetFactory
{
    public static SkillDescriptionWidget CreateSkillDescriptionWidget(SkillDescriptionWidget prefab, Transform transform, SkillInstance skill)
    {
        if (!prefab) {
            Debug.LogError("SkillDescriptionWidget prefab is not valid");
            return null;
        }

        var widget = GameObject.Instantiate(prefab, transform);
        widget.Init(skill);

        return widget;
    }
}