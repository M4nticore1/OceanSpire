using UnityEngine;

public class SelectEquipmentWidget : MonoBehaviour
{
    [SerializeField] private CustomButton button;
    [SerializeField] private WeaponItemWidget equipmentItemWidget;

    private EquipmentDefinition equipment;

    private void OnEnable()
    {
        button.onReleased.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        button.onReleased.RemoveListener(OnButtonClicked);
    }

    public void SetEquipment(ItemInstance item)
    {
        EquipmentDefinition definition = item.Definition as EquipmentDefinition;
        if (!definition) return;

        equipment = definition;
        equipmentItemWidget.SetItem(equipment);
        equipmentItemWidget.SetAmount(item);
    }

    private void OnButtonClicked()
    {
        var human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

        equipment.Equip(human);
    }
}