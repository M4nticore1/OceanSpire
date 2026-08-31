using System.Collections.Generic;
using UnityEngine;

public class ButtonsToggler : InteractionToggler
{
    private Dictionary<CustomButton, bool> buttonsDict = new();

    public override void EnableInteraction()
    {
        foreach (var button in buttonsDict) {
            button.Key.SetInteractable(button.Value);
        }

        buttonsDict.Clear();
    }

    public override void DisableInteraction()
    {
        var buttons = FindObjectsByType<CustomButton>(FindObjectsSortMode.None);

        foreach (var button in buttons) {
            if (!buttonsDict.TryAdd(button, button.IsClickable)) continue;

            button.SetInteractable(false);
        }
    }
}