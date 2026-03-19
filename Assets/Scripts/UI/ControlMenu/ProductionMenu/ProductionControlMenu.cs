using UnityEngine;

public class ProductionControlMenu : ControlMenu
{
    [SerializeField] private ProductionResourcePanel productionResourcePanelPrefab;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onClickedContextProductionButton += Open;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onClickedContextProductionButton -= Open;
    }

    protected override void OnOpen()
    {
        LocalizationItem localizedName = SelectManager.Instance.selectedComponent.GetComponent<Building>().BuildingData.LocalizationItem;
        selectedNameText.SetLocalizationItem(localizedName);
    }

    protected override void OnClose()
    {

    }
}
