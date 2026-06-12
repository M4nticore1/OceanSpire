using TMPro;
using UnityEngine;

public class EquipmentItemWidget : ResourceWidget
{
    [Header("Weapon")]
    [SerializeField] TextMeshProUGUI powerText;

    public override void SetItem(ItemDefinition definition)
    {
        base.SetItem(definition);

        WeaponDefinition weapon = definition as WeaponDefinition;
        if (!weapon) return;

        powerText.SetText(weapon.Power.ToString());
    }
}