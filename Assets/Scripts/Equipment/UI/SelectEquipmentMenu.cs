using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectEquipmentMenu : UIBehaviour
{
    [SerializeField] private ActionEquipmentWidget equipmentWidgetPrefab;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private SelectGroup selectGroup;

    private EquipmentCategory equipmentCategory;
    private List<ActionEquipmentWidget> spawnedWidgets = new();

    public void Open(EquipmentCategory category)
    {
        gameObject.SetActive(true);

        equipmentCategory = category;
        UpdateMenu(category);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void UpdateMenu(EquipmentCategory category)
    {
        ClearWidgets();
        CreateWidgets(category);
    }

    private void CreateWidgets(EquipmentCategory category)
    {
        ActionEquipmentWidget widget;
        widget = Instantiate(equipmentWidgetPrefab, layoutGroup.transform);

        var selectedCitizen = SelectManager.Instance.GetSelectedHuman();
        widget.SetEquipment(selectedCitizen.WeaponComponent);
        widget.SetSelectGroup(selectGroup);

        spawnedWidgets.Add(widget);

        foreach (var item in CityStorage.Instance.Inventory.Items) {
            if (!ShouldCreateWidget(category, item)) continue;

            var definition = item.Definition as EquipmentDefinition;

            widget = Instantiate(equipmentWidgetPrefab, layoutGroup.transform);
            widget.SetEquipment(definition);
            widget.SetSelectGroup(selectGroup);

            spawnedWidgets.Add(widget);
        }
    }
    
    private void ClearWidgets()
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            Destroy(spawnedWidgets[i].gameObject);
            spawnedWidgets.RemoveAt(i);
        }
    }

    private bool ShouldCreateWidget(EquipmentCategory category, ItemInstance item)
    {
        if (item.Amount <= 0) return false;

        var definition = item.Definition as EquipmentDefinition;
        if (!definition) return false;

        if (definition.EquipmentCategory != category) return false;

        int amount = item.Amount;

        foreach (Human human in CreaturesManager.Instance.Citizens) {
            if (human.WeaponComponent.EquipmentDefinition != definition) continue;

            amount--;
        }

        var citizen = SelectManager.Instance.GetSelectedHuman();
        if (amount <= 0 && citizen.WeaponComponent.EquipmentDefinition != definition) return false;

        return true;
    }
}