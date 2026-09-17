using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemStacksList", menuName = "Lists/ItemStacksList")]
public class ItemStacksList : ScriptableObject
{
    private static ItemStacksList instance = null;
    public static ItemStacksList Instance {
        get {
            if (instance == null) {
                instance = Resources.Load<ItemStacksList>("Lists/ItemStacksList");
            }
            return instance;
        }
    }

    [SerializeField] private ItemStackDefinition[] stackDefinitions;
    public IReadOnlyList<ItemStackDefinition> StackDefinitions => stackDefinitions;

    private Dictionary<ItemStackId, ItemStackDefinition> stackDefinitionsDict;
    public IReadOnlyDictionary<ItemStackId, ItemStackDefinition> StackDefinitionsDict => stackDefinitionsDict;

    public ItemStackDefinition GetItemStackById(ItemStackId id)
    {
        if (stackDefinitionsDict == null) {
            InitDict();
        }

        stackDefinitionsDict.TryGetValue(id, out var definition);

        return definition;
    }

    private void InitDict()
    {
        stackDefinitionsDict = new();

        foreach (var definition in stackDefinitions) {
            if (definition == null)
                continue;

            stackDefinitionsDict.Add(definition.StackId, definition);
        }
    }
}