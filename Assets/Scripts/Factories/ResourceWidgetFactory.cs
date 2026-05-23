using UnityEngine;

public static class ResourceWidgetFactory
{
    public static ResourceWidget CreateResourceWidget(ResourceWidget prefab, Transform transform)
    {
        var widget = GameObject.Instantiate(prefab, transform);

        return widget;
    }
}
