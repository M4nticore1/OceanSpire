using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ActionMenu : UIBehaviour
{
    [SerializeField] private InputStateManager inputStateManager;
    [SerializeField] protected ResourceWidget resourceWidgetPrefab;

    [SerializeField] protected TextLocalizer buildingNameTextLocalizer;
    [SerializeField] protected CustomButton actionButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] protected LayoutGroup layoutGroup;

    protected ResourceWidget[] spawnedResourceWidgets;

    protected override void OnEnable()
    {
        base.OnEnable();

        actionButton.onReleased += OnClickedActionButton;
        closeButton.onReleased += OnClickedCloseButton;
        slidePanel.onClosed += OnClose;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        actionButton.onReleased -= OnClickedActionButton;
        closeButton.onReleased -= OnClickedCloseButton;
        slidePanel.onClosed -= OnClose;
    }

    private void Action()
    {
        SelectComponent selected = SelectManager.Instance.selectedComponent;
        if (!selected) return;

        Building building = selected.GetComponent<Building>();
        if (!building) return;

        OnAction(building);
    }

    protected abstract void OnAction(Building building);

    protected abstract void CreateWidgets(Building building);

    private void Open()
    {
        slidePanel.Open();

        SelectComponent selected = SelectManager.Instance.selectedComponent;
        if (!selected) return;

        Building building = selected.GetComponent<Building>();
        if (!building) return;

        LocalizationItem localization = building.BuildingData.LocalizationItem;
        buildingNameTextLocalizer.SetLocalizationItem(localization);

        CleanWidgets();
        CreateWidgets(building);

        inputStateManager.SetGameplayInputBlocked(true);
    }

    private void Close()
    {
        slidePanel.Close();
        OnClose();
    }

    private void OnClose()
    {
        inputStateManager.SetGameplayInputBlocked(false);
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
