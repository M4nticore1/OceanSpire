using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HarvestEffectStruct
{
    public ItemDefinition itemDefinition;
    public RectTransform rectTransform;
}

public class CraftingModuleHarvestEffectManager : HarvestItemEffectManager
{
    [Header("Crafting Module Harvest")]
    [SerializeField] private Canvas playerCanvas;
    [SerializeField] private Vector3 buildingSpawnPositionOffset = new Vector3(0f, 2.5f, 0f);

    [Header("Transforms")]
    [SerializeField] private RectTransform generalTargetTransform;
    [SerializeField] private HarvestEffectStruct[] customTargetTransforms;

    private Dictionary<ItemDefinition, RectTransform> customTargetTransformsDict = new();

    protected override void Awake()
    {
        base.Awake();

        InitCustomTargetTransforms();
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        CraftingModule.OnModuleItemCollected += OnCraftingModuleItemCollected;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        CraftingModule.OnModuleItemCollected -= OnCraftingModuleItemCollected;
    }

    private void InitCustomTargetTransforms()
    {
        foreach (var transform in customTargetTransforms) {
            if (!transform.itemDefinition) continue;
            if (!transform.rectTransform) continue;

            customTargetTransformsDict.Add(transform.itemDefinition, transform.rectTransform);
        }
    }

    private void OnCraftingModuleItemCollected(CraftingModule craftingModule, CraftItemInstance craftItem)
    {
        var item = craftItem.Definition.ProduceItem;
        var worldSpawnPos = craftingModule.transform.position + buildingSpawnPositionOffset;
        var mainCamera = playerCanvas.worldCamera ? playerCanvas.worldCamera : Camera.main;

        var screenSpawnPos = mainCamera.WorldToScreenPoint(worldSpawnPos);
        screenSpawnPos.z = 0f;

        var targetPos = GetTargetPosition(item);
        targetPos.z = 0f;

        TryCreateWidget(item, playerCanvas.transform, screenSpawnPos, targetPos);
    }

    private Vector3 GetTargetPosition(ItemInstance itemInstance)
    {
        if (itemInstance == null) return Vector3.zero;

        var definition = itemInstance.Definition;
        if (!definition) return Vector3.zero;

        if (customTargetTransformsDict.TryGetValue(definition, out var transform)) {
            return transform.position;
        }
        else {
            return generalTargetTransform.position;
        }
    }
}