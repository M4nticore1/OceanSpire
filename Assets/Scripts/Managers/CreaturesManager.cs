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

    public static event Action<Human> onCitizenRegistered;
    public static event Action<Human> onCitizenUnregistered;

    public static event Action<Human> onWandererRegistered;
    public static event Action<Human> onWandererUnregistered;

    public static event Action<Human> onRaiderRegistered;
    public static event Action<Human> onRaiderUnregistered;

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
        foreach (var creature in creatures) {
            creature.Tick();
        }
    }

    // Citizen
    public void RegisterCitizen(Citizen human)
    {
        creatures.Add(human);
        citizens.Add(human);
        onCitizenRegistered?.Invoke(human);
    }

    public void UnregisterCitizen(Citizen human)
    {
        creatures.Remove(human);
        citizens.Remove(human);
        onCitizenUnregistered?.Invoke(human);
    }

    // Wanderer
    public void RegisterWanderer(Wanderer human)
    {
        creatures.Add(human);
        wanderers.Add(human);
        onWandererRegistered?.Invoke(human);
    }

    public void UnregisterWanderer(Wanderer human)
    {
        creatures.Remove(human);
        wanderers.Remove(human);
        onWandererUnregistered?.Invoke(human);
    }

    // Enemy
    public void RegisterRaider(Raider human)
    {
        creatures.Add(human);
        raiders.Add(human);
        onRaiderRegistered?.Invoke(human);
    }

    public void UnregisterRaider(Raider human)
    {
        creatures.Remove(human);
        raiders.Remove(human);
        onRaiderUnregistered?.Invoke(human);
    }
}