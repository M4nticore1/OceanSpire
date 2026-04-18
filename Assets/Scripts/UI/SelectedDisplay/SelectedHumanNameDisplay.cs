using TMPro;
using UnityEngine;

public class SelectedHumanNameDisplay : SelectedDisplay
{
    [SerializeField] private TextMeshProUGUI text;

    protected override void Display()
    {
        Human human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

        string name = human.NameHandler.GetName();

        text.SetText(name);
    }
}