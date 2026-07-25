using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSavesMenu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private SaveSlotWidget saveSlotWidgetPrefab;

    [Header("UI")]
    [SerializeField] private CreateNewWorldMenu createNewWorldMenu;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private SelectGroup selectGroup;

    private List<SaveSlotWidget> spawnedWidgets = new();

    private void OnEnable()
    {
        if (createNewWorldMenu) createNewWorldMenu.OnClosed += OnCreateMenuClosed;
        SaveSlotWidget.OnWorldDataRemoved += OnWorldDataRemoved;
    }

    private void OnDisable()
    {
        if (createNewWorldMenu) createNewWorldMenu.OnClosed -= OnCreateMenuClosed;
        SaveSlotWidget.OnWorldDataRemoved -= OnWorldDataRemoved;
    }

    private void Start()
    {
        RebuildUI();
    }

    private void RebuildUI()
    {
        ClearWidgets();

        var validSaves = new List<WorldData>();
        var saves = WorldSaveSystem.GetAllSaveData();

        if (saves != null) {
            foreach (var worldSave in saves) {
                if (worldSave != null) validSaves.Add(worldSave);
            }
        }

        int widgetsCount = validSaves.Count + 1;

        for (int i = 0; i < widgetsCount; i++) {
            var widget = Instantiate(saveSlotWidgetPrefab, layoutGroup.transform);

            if (widget.Button) {
                widget.Button.SetSelectGroup(selectGroup);
            }

            if (i < validSaves.Count) {
                widget.SetSaveData(validSaves[i]);
            }
            else {
                widget.RemoveSaveData();
            }

            spawnedWidgets.Add(widget);
        }
    }

    private void ClearWidgets()
    {
        foreach (var widget in spawnedWidgets) {
            if (widget) Destroy(widget.gameObject);
        }
        spawnedWidgets.Clear();
    }

    private void OnCreateMenuClosed()
    {
        foreach (var widget in spawnedWidgets) {
            if (!widget) continue;
            if (!widget.Button) continue;

            widget.Button.SetInteractable(true);
            widget.Button.SetState(CustomButtonState.Idle);
        }
    }

    private void OnWorldDataRemoved(SaveSlotWidget widget)
    {
        RebuildUI();
    }
}