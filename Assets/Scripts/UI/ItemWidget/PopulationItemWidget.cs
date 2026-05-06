using UnityEngine;

public class PopulationItemWidget : ResourceWidget
{
    protected override void OnEnable()
    {
        base.OnEnable();

        CreaturesManager.onCitizenRegistered += OnHumanAdded;
        CreaturesManager.onCitizenUnregistered += OnHumanRemoved;

        Human.onHumanRevived += OnHumanRevived;
        Human.onHumanDied += OnHumanDied;

        UpdateCitizensCount();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CreaturesManager.onCitizenRegistered -= OnHumanAdded;
        CreaturesManager.onCitizenUnregistered -= OnHumanRemoved;

        Human.onHumanRevived -= OnHumanRevived;
        Human.onHumanDied -= OnHumanDied;
    }

    private void UpdateCitizensCount()
    {
        if (!ItemDefinition) return;
        if (Amount == null) return;

        int amount = 0;
        int limit = CityStorage.Instance.Inventory.GetLimit(ItemDefinition.Stack);

        foreach (var citizen in CreaturesManager.Instance.Citizens) {
            if (!citizen.HealthComponent.IsAlive) continue;

            amount++;
        }

        SetAmountText(amount, limit);
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