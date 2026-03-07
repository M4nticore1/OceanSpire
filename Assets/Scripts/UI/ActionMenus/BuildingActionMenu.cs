using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BuildingActionMenu : UIBehaviour
{
    [SerializeField] protected CustomButton actionButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] protected LayoutGroup layourGroup;

    protected override void OnEnable()
    {
        base.OnEnable();

        actionButton.onReleased += OnClickedActionButton;
        closeButton.onReleased += OnClickedCloseButton;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        actionButton.onReleased -= OnClickedActionButton;
        closeButton.onReleased -= OnClickedCloseButton;
    }

    private void Action()
    {
        Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();

        if (!building) return;

        OnAction(building);
    }

    protected abstract void OnAction(Building building);

    private void Open()
    {
        slidePanel.Open();

        Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();

        if (!building) return;

        OnOpen(building);
    }

    protected abstract void OnOpen(Building building);

    private void Close()
    {
        slidePanel.Close();
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
