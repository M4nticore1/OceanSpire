using UnityEngine;

public static class DailyTaskWidgetFactory
{
    public static DailyTaskWidget CreateWidget(DailyTaskWidget prefab, Transform transform, DailyTaskInstance task)
    {
        DailyTaskWidget widget = GameObject.Instantiate(prefab, transform);
        widget.Init(task);

        return widget;
    }
}
