using UnityEngine;

public class PopulationItemWidget : ResourceWidget
{
    private void OnEnable()
    {
        CreaturesManager.onCitizenRegistered += OnHumanAdded;
        CreaturesManager.onCitizenUnregistered += OnHumanRemoved;

        Human.OnHumanRevived += OnHumanRevived;
        Human.OnHumanDied += OnHumanDied;

        Citizen.OnCitizenEvicted += OnCitizenEvicted;

        UpdateAmountAndLimit();
    }

    private void OnDisable()
    {
        CreaturesManager.onCitizenRegistered -= OnHumanAdded;
        CreaturesManager.onCitizenUnregistered -= OnHumanRemoved;

        Human.OnHumanRevived -= OnHumanRevived;
        Human.OnHumanDied -= OnHumanDied;

        Citizen.OnCitizenEvicted -= OnCitizenEvicted;
    }

    private void Start()
    {
        UpdateAmountAndLimit();
    }

    protected override void UpdateAmountAndLimit()
    {
        if (!ItemDefinition) return;

        var limit = CityStorage.Instance.Inventory.GetStack(ItemDefinition.Stack);
        SetLimit(limit);

        SetAmountText(CalculateAmountsSum(), limit.Amount);
    }

    protected override int CalculateAmountsSum()
    {
        int amount = 0;
        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            if (citizen.IsEvicted) continue;
            if (!citizen.HealthComponent.IsAlive) continue;

            amount++;
        }

        return amount;
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