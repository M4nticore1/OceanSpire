using UnityEngine;
using UnityEngine.UI;

public class EquipmentMenu : ControlMenu
{
    [SerializeField] private EquipmentCategory equipmentCategory;
    [SerializeField] private WeaponItemWidget equipmentWidget;
    [SerializeField] private LayoutGroup layoutGroup;

    protected override void Start()
    {
        base.Start();

        CreateWidgets();
    }

    protected override void OnOpen()
    {

    }

    protected override void OnClose()
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

            if (weapon.Category != equipmentCategory) return;

            var widget = Instantiate(equipmentWidget, layoutGroup.transform);
            widget.SetItem(weapon);
            widget.SetAmount(item);
            widget.SetLimit(item.Stack);
        }
    }
}