using TMPro;
using UnityEngine;

public class WeaponItemWidget : ResourceWidget
{
    [Header("Weapon")]
    [SerializeField] TextMeshProUGUI damageText;

    public override void SetAmount(IItemAmount item)
    {
        base.SetAmount(item);

        WeaponDefinition weapon = ItemDefinition as WeaponDefinition;
        if (weapon == null) return;

        damageText.SetText(weapon.Damage.ToString());
    }
}