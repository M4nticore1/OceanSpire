using UnityEngine;
using UnityEngine.UI;

public class EquipmentMenu : ControlMenu
{
    [Header("Equipment Menu")]
    [SerializeField] private EquipmentCategory equipmentCategory;
    [SerializeField] private EquipmentItemWidget equipmentWidget;
    [SerializeField] private LayoutGroup layoutGroup;

    private Citizen citizen;

    private void Start()
    {
        CreateWidgets();
    }

    protected override void UpdateMenu()
    {

    }

    protected override ILocalizable GetTargetNameText()
    {
        return citizen;
    }

    protected override ILocalizable GetTargetDescriptionText()
    {
        return null;
    }

    public void Show(Citizen citizen)
    {
        if (citizen == null) {
            Debug.LogError($"[{nameof(EquipmentMenu)}] Citizen is not valid!");
            return;
        }

        this.citizen = citizen;
        Show();
    }

    private void CreateWidgets()
    {
        foreach (var item in CityStorage.Instance.Inventory.Items) {
            var weapon = item.Definition as WeaponDefinition;
            if (weapon == null) return;

            if (weapon.EquipmentCategory != equipmentCategory) return;

            var widget = Instantiate(equipmentWidget, layoutGroup.transform);
            widget.SetItemDefinition(weapon);
            widget.AddAmount(item);
            widget.SetLimit(item.Stack);
        }
    }
}