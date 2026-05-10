using UnityEngine;

public class EquipmentPanel : MonoBehaviour
{
    [SerializeField] private EquipmentWidget weaponWidget;

    public void SetWeapon(EquipmentComponent equipmentComponent)
    {
        weaponWidget.SetEquipment(equipmentComponent);
    }
}