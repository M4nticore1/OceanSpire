using System.Collections;
using TMPro;
using UnityEngine;

public class ActionEquipmentWidget : EquipmentWidget
{
    protected override void OnEnable()
    {
        base.OnEnable();

        Button.OnSelected.AddListener(OnButtonSelected);
        Button.OnDeselected.AddListener(OnButtonDeselected);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Button.OnSelected.RemoveListener(OnButtonSelected);
        Button.OnDeselected.RemoveListener(OnButtonDeselected);
    }

    public override void SetEquipmentComponent(EquipmentComponent component)
    {
        base.SetEquipmentComponent(component);

        UpdateSelected(null);
    }

    public override void SetEquipmentDefinition(EquipmentDefinition definition)
    {
        base.SetEquipmentDefinition(definition);

        UpdateSelected(definition);
        GetAmountText(definition);
    }

    public void SetSelectGroup(SelectGroup selectGroup)
    {
        Button.SetSelectGroup(selectGroup);
    }

    protected override void OnClicked()
    {
        var human = SelectManager.Instance.GetSelectedHuman();
        if (!human) {
            Debug.Log($"Human not found at {name}");
            return;
        }

        if (equipmentDefinition) {
            equipmentDefinition.Equip(human);
        }
        else if (equipmentComponent) {
            equipmentComponent.SetEquipmentAndApply(null);
        }
    }

    private void UpdateSelected(EquipmentDefinition definition)
    {
        var citizen = SelectManager.Instance.GetSelectedHuman();
        if (!citizen) return;

        bool showSelect = citizen.WeaponComponent.EquipmentDefinition == definition || (citizen.WeaponComponent.EquipedDefaultEquipement() && definition == null);
        if (!showSelect) return;

        Button.SetState(CustomButtonState.Selected);
        Button.EndTransitionAnimation();
    }

    protected override string GetAmountText(EquipmentDefinition definition)
    {
        if (!definition) return null;

        var selectedCitizen = SelectManager.Instance.GetSelectedHuman();
        int amount = CityStorage.Instance.Inventory.GetItem(definition.ItemId).Amount;

        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            if (citizen == selectedCitizen) continue;
            if (citizen.WeaponComponent.EquipmentDefinition != definition) continue;

            amount--;
        }

        return amount.ToString();
    }

    private void OnButtonSelected()
    {
        StartCoroutine(UpdateAmountCoroutine());
    }

    private void OnButtonDeselected()
    {
        StartCoroutine(UpdateAmountCoroutine());
    }

    private IEnumerator UpdateAmountCoroutine()
    {
        yield return new WaitForEndOfFrame();

        GetAmountText(equipmentDefinition);
    }
}