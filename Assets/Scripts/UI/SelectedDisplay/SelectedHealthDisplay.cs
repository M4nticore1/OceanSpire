using UnityEngine;

public class SelectedHealthDisplay : SelectedDisplay
{
    [SerializeField] private HealthDisplay healthDisplay;

    protected override void OnShow(SelectComponent selectComponent)
    {
        base.OnShow(selectComponent);

        var health = selectComponent.GetComponent<HealthComponent>();
        if (!health) {
            Debug.LogError("healthComponent is not valid");
            return;
        }

        healthDisplay.SetHealthComponent(health);
        healthDisplay.UpdateHealth();
    }

    protected override void OnHide(SelectComponent selectComponent)
    {
        base.OnHide(selectComponent);

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