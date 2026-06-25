using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BuildingMenu : UIBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;
    protected ResourceWidget ResourceWidgetPrefab => resourceWidgetPrefab;

    [SerializeField] private SlidePanel slidePanel;

    [SerializeField] private Image buildingImage;
    protected Image BuildingImage => buildingImage;

    [SerializeField] private CustomButton actionButton;
    public CustomButton ActionButton => actionButton;

    [SerializeField] private CustomButton closeButton;

    [SerializeField] private GridLayoutGroup layoutGroup;
    protected GridLayoutGroup LayoutGroup => layoutGroup;

    [SerializeField] private TextLocalizer targetLocalizer;

    protected List<ResourceWidget> spawnedResourceWidgets = new();

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

    protected virtual bool TrySubscribe()
    {
        if (isSubscribed) return false;

        actionButton.OnReleased.AddListener(OnClickedActionButton);
        closeButton.OnReleased.AddListener(OnClickedCloseButton);
        slidePanel.OnClosed += OnClosed;

        isSubscribed = true;
        return true;
    }

    protected virtual bool TryUnsubscribe()
    {
        if (isSubscribed) return false;

        actionButton.OnReleased.RemoveListener(OnClickedActionButton);
        closeButton.OnReleased.RemoveListener(OnClickedCloseButton);
        slidePanel.OnClosed -= OnClosed;

        isSubscribed = false;
        return true;
    }

    // IOpenable
    public void Open(Building building)
    {
        if (!building) {
            Debug.Log($"Building not found at {name}");
            return;
        }

        slidePanel.Display();
        OnOpened(building);

        ClearWidgets();
        CreateWidgets(building);
        UpdateIcon(building);

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Close()
    {
        slidePanel.Hide();
        OnClosed();
    }

    protected abstract void OnOpened(Building building);

    protected abstract void OnAction(Building building);

    protected abstract void CreateWidgets(Building building);

    protected abstract void UpdateIcon(Building building);

    protected void ClearWidgets()
    {
        for (int i = spawnedResourceWidgets.Count - 1; i >= 0; i--) {
            Destroy(spawnedResourceWidgets[i].gameObject);
            spawnedResourceWidgets.RemoveAt(i);
        }
    }

    private void OnClosed()
    {
        InputStateManager.Instance.SetGameplayInputBlocked(false);
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