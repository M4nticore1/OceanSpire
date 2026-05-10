using System.Collections;
using TMPro;
using UnityEngine;

public class ActionEquipmentWidget : EquipmentWidget
{
    [Header("Storage Equipment Widget")]
    [SerializeField] private TextMeshProUGUI amountText;

    protected override void OnEnable()
    {
        base.OnEnable();

        Button.onSelected.AddListener(OnButtonSelected);
        Button.onDeselected.AddListener(OnButtonDeselected);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Button.onSelected.RemoveListener(OnButtonSelected);
        Button.onDeselected.RemoveListener(OnButtonDeselected);
    }

    public override void SetEquipment(EquipmentComponent component)
    {
        base.SetEquipment(component);

        UpdateSelected(null);
    }

    public override void SetEquipment(EquipmentDefinition definition)
    {
        base.SetEquipment(definition);

        UpdateSelected(definition);
        UpdateAmount(definition);
    }

    public void SetSelectGroup(SelectGroup selectGroup)
    {
        Button.SetSelectGroup(selectGroup);
    }

    protected override void OnClicked()
    {
        var human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

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

    private void UpdateAmount(EquipmentDefinition definition)
    {
        if (!definition) return;

        var selectedCitizen = SelectManager.Instance.GetSelectedHuman();
        int amount = CityStorage.Instance.Inventory.GetItemById(definition.ItemId).Amount;

        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            if (citizen == selectedCitizen) continue;
            if (citizen.WeaponComponent.EquipmentDefinition != definition) continue;

            amount--;
        }

        amountText.SetText(amount.ToString());
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

        UpdateAmount(equipmentDefinition);
    }
}