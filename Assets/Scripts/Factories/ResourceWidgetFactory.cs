using UnityEngine;

public static class ResourceWidgetFactory
{
    public static ItemWidget CreateResourceWidget(ItemWidget prefab, Transform transform)
    {
        var widget = GameObject.Instantiate(prefab, transform);

        return widget;
    }
}
