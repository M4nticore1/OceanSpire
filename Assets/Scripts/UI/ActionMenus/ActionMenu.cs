using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ActionMenu : UIBehaviour, IOpenable
{
    [SerializeField] protected ResourceWidget resourceWidgetPrefab;

    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] protected TextLocalizer actionTargetText;
    [SerializeField] protected CustomButton actionButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] protected LayoutGroup layoutGroup;

    protected ResourceWidget[] spawnedResourceWidgets;

    private bool isSubscribed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        TrySubscribe();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        TryUnsubscribe();
    }

    protected override void Start()
    {
        base.Start();

        TrySubscribe();
    }

    protected virtual bool Subscribe()
    {
        actionButton.OnReleased.AddListener(OnClickedActionButton);
        closeButton.OnReleased.AddListener(OnClickedCloseButton);
        slidePanel.OnClosed += OnClosed;

        return true;
    }

    protected virtual bool Unsubscribe()
    {
        actionButton.OnReleased.RemoveListener(OnClickedActionButton);
        closeButton.OnReleased.RemoveListener(OnClickedCloseButton);
        slidePanel.OnClosed += OnClosed;

        return true;
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!Subscribe()) return;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (isSubscribed) return;
        if (!Unsubscribe()) return;

        isSubscribed = false;
    }

    // IOpenable
    public void Open()
    {
        slidePanel.Open();
        OnOpened();

        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        LocalizationItem localization = building.BuildingData.LocalizationItem;
        actionTargetText.SetLocalizationItem(localization);
        actionTargetText.UpdateText();

        CleanWidgets();
        CreateWidgets(building);

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Close()
    {
        slidePanel.Close();
        OnClosed();
    }

    protected abstract void OnOpened();

    protected abstract void OnAction(Building building);

    protected abstract void CreateWidgets(Building building);

    private void OnClosed()
    {
        InputStateManager.Instance.SetGameplayInputBlocked(false);
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

    private void Action()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        OnAction(building);
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
