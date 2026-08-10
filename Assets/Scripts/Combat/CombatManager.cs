using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private List<AttackComponent> combatComponents = new();

    private void Awake()
    {
        if (Instance) {
            Debug.LogError($"[{nameof(CombatManager)}] Another instance already exists in the scene! Destroying this.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        for (int i = combatComponents.Count - 1; i >= 0; i--) {
            var component = combatComponents[i];
            if (component == null) {
                combatComponents.RemoveAt(i);
                continue;
            }

            component.Tick();
        }
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
}