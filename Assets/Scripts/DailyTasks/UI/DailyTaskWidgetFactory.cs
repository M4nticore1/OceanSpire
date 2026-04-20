using UnityEngine;

public static class DailyTaskWidgetFactory
{
    public static DailyTaskWidget CreateWidget(DailyTaskWidget prefab, Transform transform)
    {
        DailyTaskWidget widget = GameObject.Instantiate(prefab, transform);
        widget.Init();

        return widget;
    }
}
