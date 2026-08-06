using System;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesManager : MonoBehaviour
{
    public static CreaturesManager Instance { get; private set; }

    private List<Creature> creatures = new();
    public IReadOnlyList<Creature> Creatures => creatures;

    private List<Citizen> citizens = new();
    public IReadOnlyList<Citizen> Citizens => citizens;

    private List<Wanderer> wanderers = new();
    public IReadOnlyList<Wanderer> Wanderers => wanderers;

    private List<Raider> raiders = new();
    public IReadOnlyList<Raider> Raiders => raiders;

    public event Action<Human> OnCitizenRegistered;
    public event Action<Human> OnCitizenUnregistered;

    public event Action<Human> OnWandererRegistered;
    public event Action<Human> OnWandererUnregistered;

    public event Action<Human> OnRaiderRegistered;
    public event Action<Human> OnRaiderUnregistered;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        for (int i = creatures.Count - 1; i >= 0; i--) {
            var creature = creatures[i];

            if (!creature) {
                creatures.RemoveAt(i);
                continue;
            }

            creature.Tick();
        }
    }

    // Citizen
    public void RegisterCitizen(Citizen human)
    {
        creatures.Add(human);
        citizens.Add(human);
        OnCitizenRegistered?.Invoke(human);
    }

    public void UnregisterCitizen(Citizen human)
    {
        creatures.Remove(human);
        citizens.Remove(human);
        OnCitizenUnregistered?.Invoke(human);
    }

    // Wanderer
    public void RegisterWanderer(Wanderer human)
    {
        creatures.Add(human);
        wanderers.Add(human);
        OnWandererRegistered?.Invoke(human);
    }

    public void UnregisterWanderer(Wanderer human)
    {
        creatures.Remove(human);
        wanderers.Remove(human);
        OnWandererUnregistered?.Invoke(human);
    }

    // Enemy
    public void RegisterRaider(Raider human)
    {
        creatures.Add(human);
        raiders.Add(human);
        OnRaiderRegistered?.Invoke(human);
    }

    public void UnregisterRaider(Raider human)
    {
        creatures.Remove(human);
        raiders.Remove(human);
        OnRaiderUnregistered?.Invoke(human);
    }
}