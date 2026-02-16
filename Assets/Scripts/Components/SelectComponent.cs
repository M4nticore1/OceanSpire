using UnityEngine;

public class SelectComponent : MonoBehaviour, IClickable
{
    public bool isSelected { get; private set; } = false;

    private void OnEnable()
    {
        EventBus.onSelectedComponent += OnSelectedComponent;
    }

    private void OnDisable()
    {
        EventBus.onSelectedComponent -= OnSelectedComponent;
    }

    public void Click()
    {
        if (isSelected) {
            Deselect();
        }
        else {
            Select();
        }
    }

    public bool CanClick()
    {
        return true;
    }

    private void Select()
    {
        isSelected = true;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Outlined");
        }
        EventBus.InvokeSelectedObject(this);
    }

    private void Deselect()
    {
        isSelected = false;
        foreach (GameObject child in GameUtils.GetAllChildren(transform)) {
            child.layer = LayerMask.NameToLayer("Default");
        }
        EventBus.InvokeDeselectedObject(this);
    }

    private void OnSelectedComponent(SelectComponent component)
    {
        if (!isSelected) return;
        if (component == this) return;

        Deselect();
    }
}
