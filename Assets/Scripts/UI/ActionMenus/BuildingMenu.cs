using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BuildingMenu : UIBehaviour
{
    [Header("Building Menu")]
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

    protected Building building { get; private set; }
    private bool isSubscribed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        TrySubscribe();
        UpdateBuildButtonEnabled();
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

        this.building = building;

        slidePanel.Open();
        OnOpened(building);

        ClearWidgets();
        CreateWidgets(building);
        UpdateIcon(building);
        UpdateBuildButtonEnabled();

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Close()
    {
        slidePanel.Close();
        OnClosed();
    }

    protected abstract void OnOpened(Building building);

    protected abstract void OnAction(Building building);

    protected abstract void CreateWidgets(Building building);

    protected abstract void UpdateIcon(Building building);

    protected virtual bool ShouldEnableButton()
    {
        if (!building) return false;

        return true;
    }

    protected void ClearWidgets()
    {
        for (int i = spawnedResourceWidgets.Count - 1; i >= 0; i--) {
            Destroy(spawnedResourceWidgets[i].gameObject);
            spawnedResourceWidgets.RemoveAt(i);
        }
    }

    private void UpdateBuildButtonEnabled()
    {
        if (ShouldEnableButton()) {
            actionButton.SetState(CustomButtonState.Idle);
            actionButton.EndTransitionAnimation();
        }
        else {
            actionButton.SetState(CustomButtonState.Disabled);
            actionButton.EndTransitionAnimation();
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