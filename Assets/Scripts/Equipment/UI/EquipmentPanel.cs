using UnityEngine;

public class EquipmentPanel : MonoBehaviour
{
    [SerializeField] private EquipmentWidget weaponWidget;

    private void OnEnable()
    {
        var citizen = SelectManager.Instance.GetSelectedHuman();
        if (!citizen) {
            Debug.Log($"Selected Citizen not found at {name}");
            return;
        }

        var weaponComponent = citizen.WeaponComponent;
        if (!weaponComponent) return;

        SetWeapon(weaponComponent);
    }

    public void SetWeapon(EquipmentComponent equipmentComponent)
    {
        Debug.Log($"Set {equipmentComponent.gameObject}");
        weaponWidget.SetEquipmentComponent(equipmentComponent);
        weaponWidget.SetEquipmentDefinition(equipmentComponent.EquipmentDefinition);
    }
}