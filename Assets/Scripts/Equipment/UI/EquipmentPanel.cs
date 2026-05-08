using UnityEngine;

public class EquipmentPanel : MonoBehaviour
{
    [SerializeField] private EquipmentWidget weaponWidget;

    public void SetWeapon(WeaponEquipment weaponEquipment)
    {
        weaponWidget.SetEquipment(weaponEquipment.CurrentDefinition);
    }
}