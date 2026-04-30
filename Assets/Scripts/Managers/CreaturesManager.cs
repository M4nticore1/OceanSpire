using System;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesManager : MonoBehaviour
{
    public static CreaturesManager Instance { get; private set; }

    private List<Human> citizens = new();
    public IReadOnlyList<Human> Citizens => citizens.AsReadOnly();

    private List<Human> wanderers = new();
    public IReadOnlyList<Human> Wanderers => wanderers.AsReadOnly();

    private List<Human> raiders = new();
    public IReadOnlyList<Human> Raiders => raiders.AsReadOnly();

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

    // Citizen
    public void RegisterCitizen(Human human)
    {
        citizens.Add(human);
        onCitizenRegistered?.Invoke(human);
    }

    public void UnregisterCitizen(Human human)
    {
        citizens.Remove(human);
        onCitizenUnregistered?.Invoke(human);
    }

    // Wanderer
    public void RegisterWanderer(Human human)
    {
        wanderers.Add(human);
        onWandererRegistered?.Invoke(human);
    }

    public void UnregisterWanderer(Human human)
    {
        wanderers.Remove(human);
        onWandererUnregistered?.Invoke(human);
    }

    // Enemy
    public void RegisterRaider(Human human)
    {
        raiders.Add(human);
        onRaiderRegistered?.Invoke(human);
    }

    public void UnregisterRaider(Human human)
    {
        raiders.Remove(human);
        onRaiderUnregistered?.Invoke(human);
    }
}