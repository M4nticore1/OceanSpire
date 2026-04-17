using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenu : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private Transform contextMenuRoot;

    [SerializeField] private TextLocalizer labelText;
    [SerializeField] private TextLocalizer additionalText;

    [SerializeField] private LocalizationItem levelLocalization;

    protected override void OnEnable()
    {
        base.OnEnable();

        SelectManager.onComponentSelected += OnComponentSelected;
        SelectManager.onComponentDeselected += OnComponentDeselected;
        EventBus.onPlayerClicked += OnPlayerClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SelectManager.onComponentSelected -= OnComponentSelected;
        SelectManager.onComponentDeselected -= OnComponentDeselected;
        EventBus.onPlayerClicked -= OnPlayerClicked;
    }

    // Open/Close
    private void Open()
    {
        slidePanel.Open();
    }

    private void Close()
    {
        slidePanel.Close();
    }

    private void AssignText()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (building) {
            labelText.SetLocalizationItem(building.BuildingData.LocalizationItem);
            labelText.UpdateText();

            ILocalizable localizable = building.GetComponent<ILocalizable>();
            additionalText.SetLocalizationItem(levelLocalization);
            additionalText.SetPlaceHolderLocalization(localizable);
            additionalText.UpdateText();

            return;
        }

        Human human = SelectManager.Instance.GetSelectedHuman();
        if (human) {
            return;
        }
    }

    // Events
    private void OnComponentSelected(SelectComponent selected)
    {
        Open();
        AssignText();
    }

    private void OnComponentDeselected(SelectComponent selected)
    {
        Close();
    }

    private void OnPlayerClicked(GameObject clicked)
    {
        SelectComponent selectComponent = clicked?.GetComponent<SelectComponent>();
        if (selectComponent && selectComponent.isSelected) return;

        Close();
    }
}