using UnityEngine;

public static class CitizenWidgetFactory
{
    public static CitizenWidget CreateWidget(CitizenWidget prefab, Transform transform, Citizen human)
    {
        var widget = GameObject.Instantiate(prefab, transform);
        widget.Init(human);

        return widget;
    }
}
