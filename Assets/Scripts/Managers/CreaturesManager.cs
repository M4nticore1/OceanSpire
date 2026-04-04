using System;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesManager : MonoBehaviour
{
    public static CreaturesManager instance { get; private set; }

    public List<Human> citizens { get; private set; } = new List<Human>();
    public List<Human> wanderers { get; private set; } = new List<Human>();
    public List<Human> enemies { get; private set; } = new List<Human>();

    public static event Action<Human> onCitizenAdded;
    public static event Action<Human> onCitizenRemoved;

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
        onCitizenAdded?.Invoke(human);
    }

    public void UnregisterCitizen(Human human)
    {
        citizens.Remove(human);
        onCitizenRemoved?.Invoke(human);
    }

    // Wanderer
    public void RegisterWanderer(Human human)
    {
        wanderers.Add(human);
    }

    public void UnregisterWanderer(Human human)
    {
        wanderers.Remove(human);
    }

    // Enemy
    public void RegisterRaider(Human human)
    {
        enemies.Add(human);
    }

    public void UnregisterRaider(Human human)
    {
        enemies.Remove(human);
    }
}