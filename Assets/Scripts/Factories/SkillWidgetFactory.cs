using UnityEngine;

public static class SkillWidgetFactory
{
    public static SkillWidget CreateSkillWidget(SkillWidget prefab, Transform transform, SkillInstance skill)
    {
        SkillWidget widget = GameObject.Instantiate(prefab, transform);
        widget.Init(skill);

        return widget;
    }
}
