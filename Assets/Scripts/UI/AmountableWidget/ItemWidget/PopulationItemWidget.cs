using UnityEngine;

public class PopulationItemWidget : ItemWidget
{
    private CreaturesManager creaturesManager => CreaturesManager.Instance;

    protected override void OnEnable()
    {
        base.OnEnable();

        creaturesManager.OnCitizenRegistered += OnHumanAdded;
        creaturesManager.OnCitizenUnregistered += OnHumanRemoved;

        Human.OnHumanRevived += OnHumanRevived;
        Human.OnHumanDied += OnHumanDied;

        Citizen.OnCitizenEvicted += OnCitizenEvicted;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        creaturesManager.OnCitizenRegistered -= OnHumanAdded;
        creaturesManager.OnCitizenUnregistered -= OnHumanRemoved;

        Human.OnHumanRevived -= OnHumanRevived;
        Human.OnHumanDied -= OnHumanDied;

        Citizen.OnCitizenEvicted -= OnCitizenEvicted;
    }

    protected override void Start()
    {
        base.Start();

        if (ItemDefinition == null) return;
        if (cityStorage == null) return;

        var inventory = cityStorage.Inventory;
        if (inventory == null) return;

        var limit = inventory.GetStack(ItemDefinition.StackDefinition.StackId);
        SetLimit(limit);
    }

    protected override int CalculateAmountsSum()
    {
        int amount = 0;
        foreach (var citizen in creaturesManager.Citizens) {
            if (citizen == null) continue;
            if (citizen.IsEvicted) continue;
            if (!citizen.HealthComponent.IsAlive) continue;

            amount++;
        }

        return amount;
    }

    private void OnHumanAdded(Human human)
    {
        UpdateAmountAndLimitText();
    }

    private void OnHumanRemoved(Human human)
    {
        UpdateAmountAndLimitText();
    }

    private void OnHumanRevived(Human human)
    {
        UpdateAmountAndLimitText();
    }

    private void OnHumanDied(Human human)
    {
        UpdateAmountAndLimitText();
    }

    private void OnCitizenEvicted(Citizen citizen)
    {
        UpdateAmountAndLimitText();
    }
}