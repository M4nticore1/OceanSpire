using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwimmingDriftingLoot : DriftingLoot
{
    public SwimmingDriftingLootDefinition SwimmingDefinition => Definition as SwimmingDriftingLootDefinition;

    public event Action OnCollected;
    public static event Action<SwimmingDriftingLoot> OnGlobalCollected;

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

        DriftingLootManager.Instance.RegisterSwimmingDriftingLoot(this);
    }

    public List<ItemInstance> TakeItems()
    {
        Destroy(gameObject);

        OnCollected?.Invoke();
        OnGlobalCollected?.Invoke(this);

        return CreateRandomLoot();
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
        };
    }

    private List<ItemInstance> CreateRandomLoot()
    {
        var containedLoot = new List<ItemInstance>();
        var lootTable = SwimmingDefinition.LootTable;

        for (int i = 0; i < lootTable.Length; i++) {
            var chance = UnityEngine.Random.Range(0f, 1f);
            var loot = lootTable[i];

            if (chance > loot.dropChance) continue;

            int itemAmount = UnityEngine.Random.Range(loot.minAmount, loot.maxAmount + 1);
            var item = new ItemInstance(loot.itemData);
            item.SetAmount(itemAmount);

            containedLoot.Add(item);
        }

        return containedLoot;
    }
}