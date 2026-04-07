using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ActionMenu : UIBehaviour, IOpenable
{
    [SerializeField] protected ResourceWidget resourceWidgetPrefab;

    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] protected TextLocalizer buildingNameTextLocalizer;
    [SerializeField] protected CustomButton actionButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] protected LayoutGroup layoutGroup;

    protected ResourceWidget[] spawnedResourceWidgets;

    protected override void OnEnable()
    {
        base.OnEnable();

        actionButton.onReleased += OnClickedActionButton;
        closeButton.onReleased += OnClickedCloseButton;
        slidePanel.onClosed += OnClosed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        actionButton.onReleased -= OnClickedActionButton;
        closeButton.onReleased -= OnClickedCloseButton;
        slidePanel.onClosed -= OnClosed;
    }

    private void Action()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        OnAction(building);
    }

    protected abstract void OnAction(Building building);

    protected abstract void CreateWidgets(Building building);

    // IOpenable
    public void Open()
    {
        slidePanel.Open();

        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        LocalizationItem localization = building.BuildingData.LocalizationItem;
        buildingNameTextLocalizer.SetLocalizationItem(localization);

        CleanWidgets();
        CreateWidgets(building);

        InputStateManager.instance.SetGameplayInputBlocked(true);
    }

    public void Close()
    {
        slidePanel.Close();
        OnClosed();
    }

    private void OnClosed()
    {
        InputStateManager.instance.SetGameplayInputBlocked(false);
    }

    protected void CleanWidgets()
    {
        if (spawnedResourceWidgets == null)
            return;

        foreach (var widget in spawnedResourceWidgets) {
            Destroy(widget.gameObject);
        }
    }

    protected void OnContextClickedButton()
    {
        Open();
    }

    private void OnClickedActionButton()
    {
        Action();
        Close();
    }

    private void OnClickedCloseButton()
    {
        Close();
    }
}
