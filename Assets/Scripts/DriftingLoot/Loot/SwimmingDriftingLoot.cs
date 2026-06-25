using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwimmingDriftingLoot : DriftingLoot
{
    public SwimmingDriftingLootDefinition SwimmingDefinition => Definition as SwimmingDriftingLootDefinition;
    private ItemInstance[] containedLoot;

    public static event Action<DriftingLoot> OnContainerTaken;

    protected override void OnDisable()
    {
        DriftingLootManager.Instance.UnregisterSwimmingDriftingLoot(this);
    }

    protected override void OnInit(DriftingLootData driftingLootData)
    {
        base.OnInit(driftingLootData);

        var swimmingDriftingLootData = driftingLootData as SwimmingDriftingLootData;

        if (swimmingDriftingLootData == null) {
            Debug.Log($"swimmingDriftingLootData not found at {name}");
            Destroy(gameObject);
            return;
        }

        SetContainedLoot(ItemInstance.Create(swimmingDriftingLootData.Items != null ? swimmingDriftingLootData.Items : CreateRandomLootData()));
        DriftingLootManager.Instance.RegisterSwimmingDriftingLoot(this);
    }

    public ItemInstance[] TakeItems()
    {
        Destroy(gameObject);
        OnContainerTaken?.Invoke(this);

        return containedLoot;
    }

    public ItemInstance[] GetContainedLoot()
    {
        return containedLoot.ToArray();
    }

    public override DriftingLootData CreateData()
    {
        return SwimmingDriftingLootData.Create(this);
    }

    public override DriftingLootData CreateRandomData()
    {
        if (!SwimmingDefinition) {
            Debug.Log($"SwimmingDefinition not found at {gameObject}");
            return null;
        }

        return new SwimmingDriftingLootData()
        {
            Id = (int)Definition.Id,
            Position = new Vector3Data(transform.position),
            Rotation = new Vector3Data(transform.rotation.eulerAngles),
            MeshId = MeshId,
            Items = CreateRandomLootData()
        };
    }

    private void SetContainedLoot(ItemInstance[] items)
    {
        containedLoot = items;
    }

    private ItemData[] CreateRandomLootData()
    {
        var containedLoot = new List<ItemData>();
        var lootTable = SwimmingDefinition.LootTable;

        for (int i = 0; i < lootTable.Length; i++) {
            int chance = UnityEngine.Random.Range(0, 100);

            if (chance > lootTable[i].dropChance) continue;

            int itemAmount = UnityEngine.Random.Range(lootTable[i].minAmount, lootTable[i].maxAmount);
            var item = new ItemInstance(lootTable[i].itemData);
            item.SetAmount(itemAmount);

            containedLoot.Add(ItemData.Create(item));
        }

        return containedLoot.ToArray();
    }
}