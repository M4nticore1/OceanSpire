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

        UpdateCitizensCount();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CreaturesManager.onCitizenRegistered -= OnHumanAdded;
        CreaturesManager.onCitizenUnregistered -= OnHumanRemoved;

        Human.OnHumanRevived -= OnHumanRevived;
        Human.OnHumanDied -= OnHumanDied;
    }

    protected override void Start()
    {
        base.Start();

        UpdateCitizensCount();
    }

    private void UpdateCitizensCount()
    {
        if (!ItemDefinition) return;

        int amount = 0;
        var limit = CityStorage.Instance.Inventory.GetStack(ItemDefinition.Stack);
        SetLimit(limit);

        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            if (!citizen.HealthComponent.IsAlive) continue;

            amount++;
        }

        SetAmountText(amount, limit.Amount);
    }

    private void OnHumanAdded(Human human)
    {
        UpdateCitizensCount();
    }

    private void OnHumanRemoved(Human human)
    {
        UpdateCitizensCount();
    }

    private void OnHumanRevived(Human human)
    {
        UpdateCitizensCount();
    }

    private void OnHumanDied(Human human)
    {
        UpdateCitizensCount();
    }
}