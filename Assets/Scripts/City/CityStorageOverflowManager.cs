using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CityStorageOverflowManager : MonoBehaviour, ILocalizable
{
    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private List<ItemStackInstance> overflowingStacks = new();
    public IReadOnlyList<ItemStackInstance> OverflowingStacks => overflowingStacks;

    public event Action<ItemStackInstance> OnOverflowingStackAdded;
    public event Action<ItemStackInstance> OnOverflowingStackRemoved;

    private void OnEnable()
    {
        cityStorage.Inventory.OnStackItemAmountChanged += HandleStackAmountChanged;
    }

    private void OnDisable()
    {
        cityStorage.Inventory.OnStackItemAmountChanged -= HandleStackAmountChanged;
    }

    public Dictionary<string, string> GetLocalization()
    {
        var sb = new StringBuilder();
        var count = overflowingStacks.Count;

        for (int i = 0; i < count; i++) {
            var stack = overflowingStacks[i];
            if (stack == null) continue;

            var stackName = LocalizationManager.Instance.GetLocalizedText(stack.Definition.NameLocalizationItem);
            sb.Append(stackName);

            if (i < count - 1) {
                sb.Append(", ");
            }
        }

        return new Dictionary<string, string>()
        {
            { "overflowingStacks", sb.ToString() },
            { "overflowingStacksCount", overflowingStacks.Count.ToString() }
        };
    }

    private void AddOverflowingStack(ItemStackInstance stack)
    {
        if (stack == null) return;

        overflowingStacks.Add(stack);
        OnOverflowingStackAdded?.Invoke(stack);
    }

    private void RemovedOverflowingStack(ItemStackInstance stack)
    {
        if (stack == null) return;

        overflowingStacks.Remove(stack);
        OnOverflowingStackRemoved?.Invoke(stack);
    }

    private void HandleStackAmountChanged(ItemStackInstance stack)
    {
        if (stack == null) return;

        if (stack.IsOverflowed) {
            AddOverflowingStack(stack);
        }
        else {
            RemovedOverflowingStack(stack);
        }
    }
}