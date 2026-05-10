using System.Collections.Generic;
using UnityEngine;

public class SelectGroup : MonoBehaviour
{
    private List<CustomButton> buttons = new();

    public void AddButton(CustomButton button)
    {
        if (buttons.Contains(button)) return;

        buttons.Add(button);
    }

    public void RemoveButton(CustomButton button)
    {
        buttons.Remove(button);
    }

    public void OnButtonSelected(CustomButton selectedButton)
    {
        if (!buttons.Contains(selectedButton)) return;

        foreach (var button in buttons) {
            button.OnSelectGroupButtonSelected(selectedButton);
        }
    }
}