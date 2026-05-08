using TMPro;
using UnityEngine;

public class WeaponItemWidget : ResourceWidget
{
    [Header("Weapon")]
    [SerializeField] TextMeshProUGUI damageText;

    public override void SetItem(ItemDefinition definition)
    {
        base.SetItem(definition);

        WeaponDefinition weapon = definition as WeaponDefinition;
        if (!weapon) return;

        damageText.SetText(weapon.Damage.ToString());
    }
}