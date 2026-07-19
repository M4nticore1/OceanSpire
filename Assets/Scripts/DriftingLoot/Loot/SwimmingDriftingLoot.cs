using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwimmingDriftingLoot : DriftingLoot
{
    public SwimmingDriftingLootDefinition SwimmingDefinition => Definition as SwimmingDriftingLootDefinition;

    [Header("Swimming")]
    [SerializeField] private FocusComponent focusComponent;
    public FocusComponent FocusComponent => focusComponent;

    [field: SerializeField] public Boat TargetBoat {  get; private set; }

    public event Action OnCollected;
    public static event Action<SwimmingDriftingLoot> OnGlobalCollected;

    protected override void OnDisable()
    {
        DriftingLootManager.Instance.UnregisterSwimmingDriftingLoot(this);
    }

    protected override void OnInit()
    {
        OnInit(SwimmingDriftingLootData.Default() ?? new SwimmingDriftingLootData());
    }

    protected override void OnInit(DriftingLootData driftingLootData)
    {
        base.OnInit(driftingLootData);

        var swimmingDriftingLootData = driftingLootData as SwimmingDriftingLootData;
        if (swimmingDriftingLootData == null) {
            Debug.Log($"[{nameof(SwimmingDriftingLoot)}] Swimming Drifting Loot Data not found!");
            Destroy(gameObject);
            return;
        }

        focusComponent.SetFocused(swimmingDriftingLootData.Focused);
        DriftingLootManager.Instance.RegisterSwimmingDriftingLoot(this);
    }

    public override DriftingLootData CreateData()
    {
        return SwimmingDriftingLootData.Create(this);
    }

    public override DriftingLootData CreateRandomData()
    {
        if (!SwimmingDefinition) {
            Debug.Log($"[{nameof(SwimmingDriftingLoot)}] Swimming Definition not found!");
            return null;
        }

        return new SwimmingDriftingLootData()
        {
            Id = Definition.Id,
            Position = new Vector3Data(transform.position),
            Rotation = new Vector3Data(transform.rotation.eulerAngles),
            MeshId = MeshId,
        };
    }

    public void SetTargetBoat(Boat boat)
    {
        if (!boat) return;

        TargetBoat = boat;
    }

    public void RemoveTargetBoat(Boat boat)
    {
        if (!boat) return;
        if (boat != TargetBoat) return;

        TargetBoat = null;
    }

    public List<ItemInstance> TakeItems()
    {
        Destroy(gameObject);

        OnCollected?.Invoke();
        OnGlobalCollected?.Invoke(this);

        return CreateRandomLoot();
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