using System.Linq;
using UnityEngine;

public class SelectGroup : MonoBehaviour
{
    [SerializeField] private CustomButton[] buttons;

    public void OnButtonSelected(CustomButton selectedButton)
    {
        if (!buttons.Contains(selectedButton)) return;

        foreach (var button in buttons) {
            button.OnSelectGroupButtonSelected(selectedButton);
        }
    }
}