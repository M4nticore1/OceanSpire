using UnityEngine;

public static class ConstructionFinishedWidgetFactory
{
    public static ConstructionFinishedWidget CreateWidget(ConstructionFinishedWidget prefab, Transform transform, ILocalizable localizable)
    {
        var widget = GameObject.Instantiate(prefab, transform);
        widget.Init(localizable);

        return widget;
    }
}