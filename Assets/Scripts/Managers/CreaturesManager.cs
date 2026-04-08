using System;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesManager : MonoBehaviour
{
    public static CreaturesManager instance { get; private set; }

    public List<Human> citizens { get; private set; } = new List<Human>();
    public List<Human> wanderers { get; private set; } = new List<Human>();
    public List<Human> enemies { get; private set; } = new List<Human>();

    public static event Action<Human> onCitizenRegistered;
    public static event Action<Human> onCitizenUnregistered;

    public static event Action<Human> onWandererRegistered;
    public static event Action<Human> onWandererUnregistered;

    public static event Action<Human> onRaiderRegistered;
    public static event Action<Human> onRaiderUnregistered;

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
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
        enemies.Add(human);
        onRaiderRegistered?.Invoke(human);
    }

    public void UnregisterRaider(Human human)
    {
        enemies.Remove(human);
        onRaiderUnregistered?.Invoke(human);
    }
}