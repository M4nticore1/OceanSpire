using UnityEngine;

public class PopulationItemWidget : ResourceWidget
{
    protected override void OnEnable()
    {
        base.OnEnable();

        CreaturesManager.onCitizenRegistered += OnHumanAdded;
        CreaturesManager.onCitizenUnregistered += OnHumanRemoved;

        Human.OnHumanRevived += OnHumanRevived;
        Human.OnHumanDied += OnHumanDied;

        Citizen.OnCitizenEvicted += OnCitizenEvicted;

        UpdateAmountAndLimit();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CreaturesManager.onCitizenRegistered -= OnHumanAdded;
        CreaturesManager.onCitizenUnregistered -= OnHumanRemoved;

        Human.OnHumanRevived -= OnHumanRevived;
        Human.OnHumanDied -= OnHumanDied;

        Citizen.OnCitizenEvicted -= OnCitizenEvicted;
    }

    protected override void Start()
    {
        base.Start();

        UpdateAmountAndLimit();
    }

    protected override void UpdateAmountAndLimit()
    {
        if (!ItemDefinition) return;

        int amount = 0;
        var limit = CityStorage.Instance.Inventory.GetStack(ItemDefinition.Stack);
        SetLimit(limit);

        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            if (citizen.IsEvicted) continue;
            if (!citizen.HealthComponent.IsAlive) continue;

            amount++;
        }

        SetAmountText(amount, limit.Amount);
    }

    private void OnHumanAdded(Human human)
    {
        UpdateAmountAndLimit();
    }

    private void OnHumanRemoved(Human human)
    {
        UpdateAmountAndLimit();
    }

    private void OnHumanRevived(Human human)
    {
        UpdateAmountAndLimit();
    }

    private void OnHumanDied(Human human)
    {
        UpdateAmountAndLimit();
    }

    private void OnCitizenEvicted(Citizen citizen)
    {
        UpdateAmountAndLimit();
    }
}