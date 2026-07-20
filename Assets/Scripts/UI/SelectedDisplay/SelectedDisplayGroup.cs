using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectedDisplayGroup : MonoBehaviour
{
    [SerializeField] private SelectedDisplay[] selectedDisplays;

    private List<SelectedDisplay> showedSelectedDisplays = new();
    private List<SelectedDisplay> displaysToHide = new();

    private void OnEnable()
    {
        foreach (var display in selectedDisplays) {
            if (!display) continue;

            display.OnShowed += OnSelectedDisplayShowed;
            display.OnHidden += OnSelectedDisplayHidden;
        }
    }

    private void OnDisable()
    {
        foreach (var display in selectedDisplays) {
            if (!display) continue;

            display.OnShowed -= OnSelectedDisplayShowed;
            display.OnHidden -= OnSelectedDisplayHidden;
        }
    }

    private void OnSelectedDisplayShowed(SelectedDisplay selectedDisplay)
    {
        if (!selectedDisplay) return;

        if (!showedSelectedDisplays.Contains(selectedDisplay)) {
            showedSelectedDisplays.Add(selectedDisplay);
        }

        if (showedSelectedDisplays.Count > 1) {
            int targetIndex = Array.IndexOf(selectedDisplays, selectedDisplay);
            if (targetIndex == -1) return;

            displaysToHide.Clear();

            foreach (var display in showedSelectedDisplays) {
                int currentIndex = Array.IndexOf(selectedDisplays, display);

                if (currentIndex > targetIndex) {
                    displaysToHide.Add(display);
                }
            }

            foreach (var display in displaysToHide) {
                display.Hide(null);
            }
        }
    }

    private void OnSelectedDisplayHidden(SelectedDisplay selectedDisplay)
    {
        if (!selectedDisplay) return;

        showedSelectedDisplays.Remove(selectedDisplay);
    }
}