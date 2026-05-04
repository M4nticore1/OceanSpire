using TMPro;
using UnityEngine;

public class SelectedHumanNameDisplay : SelectedDisplay
{
    [SerializeField] private TextMeshProUGUI text;

    protected override void TryDisplay()
    {
        Human human = SelectManager.Instance.GetSelectedHuman();
        if (!human) return;

        string name = human.NameComponent.GetName();

        text.SetText(name);
    }

    protected override void TryHide()
    {
        text.SetText("");
    }
}