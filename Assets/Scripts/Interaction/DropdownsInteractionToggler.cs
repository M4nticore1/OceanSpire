using System.Collections.Generic;
using UnityEngine;

public class DropdownsInteractionToggler : InteractionToggler
{
    private Dictionary<CustomDropdown, bool> dropdownsDict = new();

    public override void EnableInteraction()
    {
        foreach (var dropdown in dropdownsDict) {
            dropdown.Key.SetListeningClick(dropdown.Value);
        }

        dropdownsDict.Clear();
    }

    public override void DisableInteraction()
    {
        var buttons = FindObjectsByType<CustomDropdown>(FindObjectsSortMode.None);

        foreach (var dropdown in buttons) {
            if (!dropdownsDict.TryAdd(dropdown, dropdown.IsListeningClick)) continue;

            dropdown.SetListeningClick(false);
        }
    }
}