using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectEquipmentMenu : UIBehaviour
{
    [SerializeField] private EquipmentWidget equipmentWidgetPrefab;
    [SerializeField] private GridLayoutGroup layoutGroup;

    private List<EquipmentWidget> spawnedWidgets = new();

    public void Open(EquipmentCategory category)
    {
        gameObject.SetActive(true);
        ClearWidgets();
        CreateWidgets(category);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void CreateWidgets(EquipmentCategory category)
    {
        foreach (var item in CityStorage.Instance.Inventory.Items) {
            if (item.Amount <= 0) continue;

            var equipmentDef = item.Definition as EquipmentDefinition;
            if (!equipmentDef) continue;

            if (equipmentDef.Category != category) continue;

            var widget = Instantiate(equipmentWidgetPrefab, layoutGroup.transform);
            widget.SetEquipWidget(true);
            widget.SetEquipment(equipmentDef);

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
}