using UnityEngine;

public class SelectedHealthDisplay : SelectedDisplay
{
    [SerializeField] private HealthDisplay healthDisplay;

    protected override void Display(SelectComponent selectComponent)
    {
        base.Display(selectComponent);

        var health = selectComponent.GetComponent<HealthComponent>();
        if (!health) {
            Debug.LogError("healthComponent is not valid");
            return;
        }

        healthDisplay.SetHealthComponent(health);
        healthDisplay.UpdateHealth();
    }

    protected override void Hide(SelectComponent selectComponent)
    {
        base.Hide(selectComponent);

        healthDisplay.RemoveHealthComponent();
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var health = selectComponent.GetComponent<HealthComponent>();
        if (!health) return false;

        return true;
    }
}