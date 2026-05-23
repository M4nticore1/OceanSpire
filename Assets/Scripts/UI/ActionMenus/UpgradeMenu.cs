using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeMenu : UIBehaviour
{
    [SerializeField] private ResourceWidget resourceWidget;

    [SerializeField] private SlidePanel slidePanel;

    [SerializeField] private CustomButton upgradeButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private GridLayoutGroup layoutGroup;

    private UpgradeComponent upgradeComponent;

    protected override void OnEnable()
    {
        base.OnEnable();

        upgradeButton.OnReleased.AddListener(OnUpgradeButtonClicked);
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        upgradeButton.OnReleased.RemoveListener(OnUpgradeButtonClicked);
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    public void Open(UpgradeComponent upgradeComponent)
    {
        this.upgradeComponent = upgradeComponent;
        slidePanel.Open();
    }

    public void Close()
    {
        slidePanel.Close();
    }

    private void CreateWidgets(Building building)
    {
        foreach (var item in building.NextLevelData.ResourcesToBuild) {

        }
    }

    private void ClearWidgets()
    {

    }

    private void OnUpgradeButtonClicked()
    {
        upgradeComponent.Upgrade();
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }
}