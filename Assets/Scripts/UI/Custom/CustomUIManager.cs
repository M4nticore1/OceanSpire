using System.Collections.Generic;
using UnityEngine;

public class CustomUIManager : MonoBehaviour
{
    public static CustomUIManager Instance { get; private set; }

    private List<CustomUI> buttons = new();
    private List<CustomUI> dropdowns = new();

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError($"[{nameof(CustomUIManager)}] Another Custom UI Manager is alredy on the scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        for (int i = buttons.Count - 1; i >= 0; i--) {
            var button = buttons[i];
            if (button == null) {
                buttons.RemoveAt(i);
                continue;
            }

            button.Tick();
        }

        for (int i = dropdowns.Count - 1; i >= 0; i--) {
            var dropdown = dropdowns[i];
            if (dropdown == null) {
                dropdowns.RemoveAt(i);
                continue;
            }

            dropdown.Tick();
        }
    }

    // Buttons
    public void RegisterCustomButton(CustomButton button)
    {
        RegisterCustomUI(button, buttons);
    }

    public void UnregisterCustomButton(CustomButton button)
    {
        UnregisterCustomUI(button, buttons);
    }

    // Dropdowns
    public void RegisterCustomDropdown(CustomDropdown dropdown)
    {
        RegisterCustomUI(dropdown, dropdowns);
    }

    public void UnregisterCustomDropdown(CustomDropdown dropdown)
    {
        UnregisterCustomUI(dropdown, dropdowns);
    }

    // General
    private void RegisterCustomUI(CustomUI UI, List<CustomUI> UIList)
    {
        if (UI == null) return;

        UIList.Add(UI);
    }

    private void UnregisterCustomUI(CustomUI UI, List<CustomUI> UIList)
    {
        UIList.Remove(UI);
    }
}