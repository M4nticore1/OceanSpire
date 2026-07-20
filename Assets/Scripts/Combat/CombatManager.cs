using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private readonly HashSet<AttackComponent> combatComponents = new();

    private void Awake()
    {
        if (Instance) {
            Debug.LogError($"[{nameof(CombatManager)}] Another instance already exists in the scene! Destroying this.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(AttackComponent component)
    {
        if (!component) return;

        combatComponents.Add(component);
    }

    public void Unregister(AttackComponent component)
    {
        if (!component) return;

        combatComponents.Remove(component);
    }

    private void Update()
    {
        foreach (var component in combatComponents) {
            component.Tick();
        }
    }
}