using System.Collections.Generic;
using UnityEngine;

public class CraftingModulesManager : MonoBehaviour
{
    public static CraftingModulesManager Instance;

    private List<CraftingModule> craftingModules = new();
    public IReadOnlyList<CraftingModule> CraftingModules => craftingModules;

    private void Awake()
    {
        if (Instance) {
            Debug.Log($"[{nameof(CraftingModulesManager)}] Another CraftingModulesManager is already on the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterCraftingModule(CraftingModule craftingModule)
    {
        if (craftingModules.Contains(craftingModule)) return;

        craftingModules.Add(craftingModule);
    }

    public void UnregisterCraftingModule(CraftingModule craftingModule)
    {
        if (!craftingModules.Contains(craftingModule)) return;

        craftingModules.Remove(craftingModule);
    }

    private void Update()
    {
        foreach (var module in craftingModules) {
            module.Tick();
        }
    }
}