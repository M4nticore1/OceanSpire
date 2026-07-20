using System;
using System.Collections.Generic;
using UnityEngine;

public class FocusManager : MonoBehaviour
{
    public static FocusManager Instance { get; private set; }

    private HashSet<FocusPointer> focusPointers = new();
    public IReadOnlyCollection<FocusPointer> FocusPointersList => focusPointers;

    private List<FocusComponent> focusComponentsList = new();
    public IReadOnlyList<FocusComponent> FocusComponentsList => focusComponentsList;

    private Dictionary<FocusComponent, FocusPointer> focusPointersDict = new();
    public IReadOnlyDictionary<FocusComponent, FocusPointer> FocusPointersDict => focusPointersDict;

    private void OnEnable()
    {
        FocusComponent.OnFocusedChanged += OnFocusedChanged;
        FocusComponent.OnComponentDestroyed += OnFocusComponentDestroyed;
    }

    private void OnDisable()
    {
        FocusComponent.OnFocusedChanged -= OnFocusedChanged;
        FocusComponent.OnComponentDestroyed -= OnFocusComponentDestroyed;
    }

    private void Awake()
    {
        if (Instance) {
            Debug.LogError($"[{nameof(FocusManager)}] Another instance already exists in the scene! Destroying this.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        foreach (var pointer in focusPointers) {
            pointer.Tick();
        }
    }

    public void Init()
    {
        Init(FocusSystemData.Default());
    }

    public void Init(FocusSystemData focusData)
    {
        if (focusData == null) {
            Debug.LogError($"[{nameof(FocusManager)}] Focus Data is not valid!");
            Init();
            return;
        }

        if (focusData.focusedInstanceIds != null) {
            foreach (var guid in focusData.focusedInstanceIds) {
                if (guid == Guid.Empty) continue;

                var instance = InstancesManager.Instance.GetInstance(guid);
                if (!instance) continue;

                var focusComponent = instance.GetComponent<FocusComponent>();
                if (!focusComponent) continue;

                focusComponent.SetFocused(true);
            }
        }
    }

    public void RegisterPointer(FocusPointer pointer)
    {
        if (!pointer) return;

        focusPointers.Add(pointer);
    }

    public void UnregisterPointer(FocusPointer pointer)
    {
        if (!pointer) return;

        focusPointers.Remove(pointer);
    }

    private void CreateFocusPointer(FocusComponent focusComponent)
    {
        if (!focusComponent) return;

        var pointer = FocusPointerFactory.CreatePointer(focusComponent.FocusPointerPrefab, focusComponent.transform);
        if (!pointer) return;

        focusPointers.Add(pointer);
        focusPointersDict.Add(focusComponent, pointer);
    }

    private void RemoveFocusPointer(FocusComponent focusComponent)
    {
        if (!focusComponent) return;
        if (!focusPointersDict.TryGetValue(focusComponent, out var pointer)) return;

        Destroy(pointer.gameObject);
        focusPointers.Remove(pointer);
        focusPointersDict.Remove(focusComponent);
    }

    private void AddFocusComponent(FocusComponent focusComponent)
    {
        if (!focusComponent) return;

        focusComponentsList.Add(focusComponent);
    }

    private void RemoveFocusComponent(FocusComponent focusComponent)
    {
        if (!focusComponent) return;

        focusComponentsList.Remove(focusComponent);
    }

    private void UpdateFocusPointer(FocusComponent focusComponent)
    {
        if (!focusComponent) return;

        if (focusComponent.IsFocused && !focusComponentsList.Contains(focusComponent)) {
            if (focusComponentsList.Contains(focusComponent)) return;

            AddFocusComponent(focusComponent);
            CreateFocusPointer(focusComponent);
        }
        else if (focusComponentsList.Contains(focusComponent)) {
            RemoveFocusComponent(focusComponent);
            RemoveFocusPointer(focusComponent);
        }
    }

    private void OnFocusedChanged(FocusComponent focusComponent)
    {
        UpdateFocusPointer(focusComponent);
    }

    private void OnFocusComponentDestroyed(FocusComponent focusComponent)
    {
        RemoveFocusComponent(focusComponent);
    }
}