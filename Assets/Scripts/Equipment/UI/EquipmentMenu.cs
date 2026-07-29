using UnityEngine;
using UnityEngine.UI;

public class EquipmentMenu : ControlMenu
{
    [SerializeField] private EquipmentCategory equipmentCategory;
    [SerializeField] private EquipmentItemWidget equipmentWidget;
    [SerializeField] private LayoutGroup layoutGroup;

    private void Start()
    {
        CreateWidgets();
    }

    protected override void OnShow()
    {

    }

    protected override void OnHide()
    {

    }

    protected override void UpdateMenu()
    {

    }

    private void CreateWidgets()
    {
        foreach (var item in CityStorage.Instance.Inventory.Items) {
            var weapon = item.Definition as WeaponDefinition;
            if (!weapon) return;

            if (weapon.EquipmentCategory != equipmentCategory) return;

            var widget = Instantiate(equipmentWidget, layoutGroup.transform);
            widget.SetItemDefinition(weapon);
            widget.AddAmount(item);
            widget.SetLimit(item.Stack);
        }
    }
}