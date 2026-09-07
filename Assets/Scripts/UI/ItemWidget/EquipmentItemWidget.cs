using TMPro;
using UnityEngine;

public class EquipmentItemWidget : ItemWidget
{
    [Header("Weapon")]
    [SerializeField] TextMeshProUGUI powerText;

    public override void SetItemDefinition(ItemDefinition definition)
    {
        base.SetItemDefinition(definition);

        WeaponDefinition weapon = definition as WeaponDefinition;
        if (!weapon) return;

        powerText.SetText(weapon.Power.ToString());
    }
}