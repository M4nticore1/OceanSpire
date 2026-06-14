using System.Collections.Generic;
using UnityEngine;

public class CustomUIManager : MonoBehaviour
{
    public static CustomUIManager Instance { get; private set; }

    private List<CustomButton> buttons = new();

    private void Awake()
    {
        if (Instance) {
            Debug.LogError("Another Custom UI Manager is alredy on the scene");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterCustomButton(CustomButton button)
    {
        buttons.Add(button);
    }

    public void UnregisterCustomButton(CustomButton button)
    {
        buttons.Remove(button);
    }

    private void Update()
    {
        foreach (var button in buttons) {
            button.Tick();
        }
    }
}