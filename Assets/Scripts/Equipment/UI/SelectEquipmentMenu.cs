using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectEquipmentMenu : ControlMenu
{
    [Header("Select Equipment Menu")]
    [SerializeField] private ActionEquipmentWidget equipmentWidgetPrefab;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private SelectGroup selectGroup;

    private EquipmentCategory equipmentCategory;
    private EquipmentComponent equipmentComponent;
    private List<ActionEquipmentWidget> spawnedWidgets = new();

    protected override void UpdateMenu()
    {

    }

    protected override ILocalizable GetTargetNameText()
    {
        if (equipmentComponent == null) return null;

        var human = equipmentComponent.GetComponent<Human>();
        if (human == null) return null;

        return human;
    }

    protected override ILocalizable GetTargetDescriptionText()
    {
        return null;
    }

    public void Show(EquipmentComponent equipmentComponent, EquipmentCategory category)
    {
        if (equipmentComponent == null) {
            Debug.LogError($"[{nameof(SelectEquipmentMenu)}] Equipment Component is not valid!");
            return;
        }

        this.equipmentComponent = equipmentComponent;
        equipmentCategory = category;

        UpdateMenu(category);
        Show();
    }

    private void UpdateMenu(EquipmentCategory category)
    {
        ClearWidgets();
        CreateDeselectWidget();
        CreateStorageWidgets(category);
    }

    private void CreateDeselectWidget()
    {
        var widget = Instantiate(equipmentWidgetPrefab, layoutGroup.transform);
        if (widget == null) {
            Debug.LogError($"[{nameof(SelectEquipmentMenu)}] Widget is not valid!");
            return;
        }

        widget.SetSelectGroup(selectGroup);
        widget.SetEquipmentComponent(equipmentComponent);

        spawnedWidgets.Add(widget);
    }

    private void CreateStorageWidgets(EquipmentCategory category)
    {
        foreach (var item in CityStorage.Instance.Inventory.Items) {
            if (!ShouldCreateWidget(category, item)) continue;

            var definition = item.Definition as EquipmentDefinition;

            var widget = Instantiate(equipmentWidgetPrefab, layoutGroup.transform);
            widget.SetSelectGroup(selectGroup);
            widget.SetEquipmentDefinition(definition);

            spawnedWidgets.Add(widget);
        }
    }
    
    private void ClearWidgets()
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            var widget = spawnedWidgets[i];
            if (widget == null) {
                spawnedWidgets.RemoveAt(i);
                continue;
            }

            Destroy(spawnedWidgets[i].gameObject);
            spawnedWidgets.RemoveAt(i);
        }

        foreach (Transform child in layoutGroup.transform) {
            if (child == null) continue;

            Destroy(child.gameObject);
        }
    }

    private bool ShouldCreateWidget(EquipmentCategory category, ItemInstance item)
    {
        if (item == null) return false;
        if (item.Amount <= 0) return false;

        var definition = item.Definition as EquipmentDefinition;
        if (definition == null) return false;

        if (definition.EquipmentCategory != category) return false;

        var amount = item.Amount;
        foreach (var human in CreaturesManager.Instance.Citizens) {
            if (human == null) continue;
            if (human.WeaponComponent.EquipmentDefinition != definition) continue;

            amount--;
        }

        var citizen = SelectManager.Instance.GetSelectedHuman();
        if (amount <= 0 && citizen.WeaponComponent.EquipmentDefinition != definition) return false;

        return true;
    }
}