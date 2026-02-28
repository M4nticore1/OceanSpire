using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class BoatContextMenu : ContextMenuBase<Boat>
{
    [SerializeField] private TextMeshProUGUI healthText = null;
    [SerializeField] private TextMeshProUGUI weightText = null;
    [SerializeField] private HealthDisplay healthDisplay = null;

    public override void Init(Boat boat)
    {
        SetNameText(boat.BoatData.BoatName);
        SetHealthValue(boat.CurrentHealth, boat.MaxHealth);
        //SetWeight(boat.CurrentWeight, boat.MaxWeight);

        healthDisplay.SetHealthComponent(boat.Health);
    }

    public void SetHealthValue(float currentHealth, float maxHealth)
    {
        healthText.SetText("Health " + math.floor(currentHealth) + "/" + math.floor(maxHealth));
    }

    public void SetWeight(float currentWeight, float maxWeight)
    {
        weightText.SetText("Weight\n" + (int)currentWeight + "/" + (int)maxWeight);
    }
}