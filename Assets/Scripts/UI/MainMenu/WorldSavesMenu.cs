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
        createNewWorldMenu.OnClosed += OnCreateMenuClosed;
        SaveSlotWidget.OnWorldDataRemoved += OnWorldDataRemoved;
    }

    private void OnDisable()
    {
        createNewWorldMenu.OnClosed -= OnCreateMenuClosed;
        SaveSlotWidget.OnWorldDataRemoved -= OnWorldDataRemoved;
    }

    private void Start()
    {
        CreateWidgets();
    }

    private void UpdateSaveSlotsData()
    {
        var saves = WorldSaveSystem.GetAllSaveData();

        for (int i = 0; i < spawnedWidgets.Count; i++) {
            var widget = spawnedWidgets[i];
            if (!widget) {
                Debug.LogError($"[{nameof(WorldSavesMenu)}] Spawned Widget is not valid at index {i}!");
                continue;
            }

            if (saves != null && i < saves.Length && saves[i] != null) {
                widget.SetSaveData(saves[i]);
            }
            else {
                widget.RemoveSaveData();
            }
        }
    }

    private void CreateWidgets()
    {
        var worldSaves = new List<WorldData>();

        var saves = WorldSaveSystem.GetAllSaveData();
        if (saves != null) {
            foreach (var worldSave in saves) {
                if (worldSave == null) continue;

                worldSaves.Add(worldSave);
            }
        }

        var widgetsCount = worldSaves.Count + 1;

        for (int i = 0; i < widgetsCount; i++) {
            var widget = Instantiate(saveSlotWidgetPrefab, layoutGroup.transform);
            widget.Button.SetSelectGroup(selectGroup);

            if (i < worldSaves.Count) {
                widget.SetSaveData(worldSaves[i]);
            }

            spawnedWidgets.Add(widget);
        }
    }

    private void RemoveExtraSaveSlots()
    {
        var saves = WorldSaveSystem.GetAllSaveData();
        var extraSlotsCount = Mathf.Abs(spawnedWidgets.Count - 1 - (saves != null ? saves.Length : 0));
        extraSlotsCount = Mathf.Clamp(extraSlotsCount, 0, extraSlotsCount);

        for (int i = 0; i < extraSlotsCount; i++) {
            var index = spawnedWidgets.Count - i - 1;
            var widget = spawnedWidgets[index];
            Destroy(widget.gameObject);
            spawnedWidgets.RemoveAt(index);
        }
    }

    private void OnCreateMenuClosed()
    {
        foreach (var widget in spawnedWidgets) {
            if (!widget) continue;

            widget.Button.SetInteractable(true);
            widget.Button.SetState(CustomButtonState.Idle);
        }
    }

    private void OnWorldDataRemoved(SaveSlotWidget widget)
    {
        RemoveExtraSaveSlots();
        UpdateSaveSlotsData();
    }
}