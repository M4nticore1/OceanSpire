using System.Collections.Generic;
using UnityEngine;

public class EntitiesManager : MonoBehaviour
{
    public static EntitiesManager instance;

    public List<Human> citizens { get; private set; } = new List<Human>();
    public List<Human> wanderers { get; private set; } = new List<Human>();
    public List<Human> enemies { get; private set; } = new List<Human>();

    private void Awake()
    {
        instance = this;
    }

    // Citizen
    public void RegisterCitizen(Human human)
    {
        citizens.Add(human);
    }

    public void UnregisterCitizen(Human human)
    {
        citizens.Remove(human);
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
    public void RegisterEnemy(Human human)
    {
        enemies.Add(human);
    }

    public void UnregisterEnemy(Human human)
    {
        enemies.Remove(human);
    }
}