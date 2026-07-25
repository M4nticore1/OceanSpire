using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectEquipmentMenu : UIBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private ActionEquipmentWidget equipmentWidgetPrefab;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private SelectGroup selectGroup;

    private EquipmentCategory equipmentCategory;
    private List<ActionEquipmentWidget> spawnedWidgets = new();

    public void Open(EquipmentCategory category)
    {
        content.SetActive(true);

        equipmentCategory = category;
        UpdateMenu(category);
    }

    public void Close()
    {
        content.SetActive(false);
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

        var selectedCitizen = SelectManager.Instance.GetSelectedHuman();
        widget.SetSelectGroup(selectGroup);
        widget.SetEquipmentComponent(selectedCitizen.WeaponComponent);

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
            if (!widget) {
                spawnedWidgets.RemoveAt(i);
                continue;
            }

            Destroy(spawnedWidgets[i].gameObject);
            spawnedWidgets.RemoveAt(i);
        }

        foreach (Transform child in layoutGroup.transform) {
            Destroy(child.gameObject);
        }
    }

    private bool ShouldCreateWidget(EquipmentCategory category, ItemInstance item)
    {
        if (item == null) return false;
        if (item.Amount <= 0) return false;

        var definition = item.Definition as EquipmentDefinition;
        if (!definition) return false;

        if (definition.EquipmentCategory != category) return false;

        int amount = item.Amount;

        foreach (var human in CreaturesManager.Instance.Citizens) {
            if (human.WeaponComponent.EquipmentDefinition != definition) continue;

            amount--;
        }

        var citizen = SelectManager.Instance.GetSelectedHuman();
        if (amount <= 0 && citizen.WeaponComponent.EquipmentDefinition != definition) return false;

        return true;
    }
}