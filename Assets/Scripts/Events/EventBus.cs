using System;

public class EventBus
{
    public static EventBus Instance { get; private set; }

    public event Action<string> onCreateWorldButtonClicked;
    public event Action<SaveData> onLoadWorldButtonClicked;
    public event Action<BuildingPlace> onConstructionPlacePressed;
    public event Action<BuildingWidget> onBuildingWidgetBuildClicked;
    public event Action<BuildingWidget> onBuildingWidgetInformationClicked;

    public EventBus()
    {
        Instance = this;
    }

    public void InvokeConstructionPlacePressed(BuildingPlace place)
    {
        onConstructionPlacePressed?.Invoke(place);
    }

    public void InvokeBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        onBuildingWidgetBuildClicked?.Invoke(widget);
    }

    public void InvokeBuildingWidgetInformationClicked(BuildingWidget widget)
    {
        onBuildingWidgetInformationClicked?.Invoke(widget);
    }
}
