using UnityEngine;

public static class CitizenWidgetFactory
{
    public static CitizenWidget CreateWidget(CitizenWidget prefab, Transform transform, Human human)
    {
        CitizenWidget widget = GameObject.Instantiate(prefab, transform);
        widget.Init(human);

        return widget;
    }
}
