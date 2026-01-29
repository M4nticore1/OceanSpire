using UnityEngine;

public class SelectComponent : MonoBehaviour
{
    public bool isSelected { get; private set; } = false;

    public void Select()
    {
        isSelected = true;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Outlined");
        }
        EventBus.Instance.InvokeObjectSelected(this);
    }

    public void Deselect()
    {
        isSelected = false;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Default");
        }
        EventBus.Instance.InvokeObjectDeselected();
    }
}
