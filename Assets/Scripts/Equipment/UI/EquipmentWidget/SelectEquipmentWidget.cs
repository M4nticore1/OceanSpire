using UnityEngine;

public class SelectEquipmentWidget : EquipmentWidget
{
    [Header("Select Equipment Widget")]
    [SerializeField] private SelectEquipmentMenu selectEquipmentMenu;

    protected override void OnEnable()
    {
        base.OnEnable();

        EquipmentComponent.OnEquipmentComponentEquiped += OnEquipmentComponentEquiped;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EquipmentComponent.OnEquipmentComponentEquiped -= OnEquipmentComponentEquiped;
    }

    protected override string GetAmountText(EquipmentDefinition definition)
    {
        if (!definition) return null;

        var amount = CityStorage.Instance.Inventory.GetItem(definition.ItemId).Amount;
        return amount.ToString();
    }

    protected override void OnClicked()
    {
        selectEquipmentMenu.Open(EquipmentCategory);
    }

    private void OnEquipmentComponentEquiped(EquipmentComponent equipmentComponent)
    {
        if (equipmentComponent != this.equipmentComponent) return;

        SetEquipmentDefinition(equipmentComponent.EquipmentDefinition);
    }
}