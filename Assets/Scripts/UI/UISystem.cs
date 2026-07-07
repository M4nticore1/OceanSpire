using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickData
{
    public GameObject gameObject;
}

public class UISystem
{
    private static UISystem instance;
    public static UISystem Instance => instance ??= new UISystem();

    public Selectable hoveredSelectable;
    public Selectable pressedSelectable;
    public List<Selectable> selectedSelectables = new List<Selectable>();

    public UISystem()
    {
        InputListener.Instance.OnPressed += OnScreenInputPressed;
        InputListener.Instance.OnReleased += OnScreenInputReleased;
    }

    private void OnScreenInputPressed()
    {

    }

    private void OnScreenInputReleased()
    {

    }

    public void SetHoveredSelectable()
    {

    }

    public void SetPressedSelectable()
    {

    }

    public void AddSelectedSelectables(Selectable selectable)
    {

    }

    public void RemoveSelectedSelectables(Selectable selectable)
    {

    }
}
