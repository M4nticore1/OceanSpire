using TMPro;
using UnityEngine;

public class EquipmentItemWidget : ResourceWidget
{
    [Header("Weapon")]
    [SerializeField] TextMeshProUGUI powerText;

    public override void SetItemAndApply(ItemDefinition definition)
    {
        base.SetItemAndApply(definition);

        WeaponDefinition weapon = definition as WeaponDefinition;
        if (!weapon) return;

        powerText.SetText(weapon.Damage.ToString());
    }
}