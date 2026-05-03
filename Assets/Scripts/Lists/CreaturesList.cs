using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreaturesList", menuName = "GameContent/Creatures List")]
public class CreaturesList : ScriptableObject
{
    private static CreaturesList _instance;
    public static CreaturesList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<CreaturesList>("Lists/CreaturesList");
            }
            return _instance;
        }
    }

    [SerializeField] private Creature[] creatures;

    [SerializeField] private Creature[] citizens;
    [SerializeField] private Creature[] wanderers;
    [SerializeField] private Creature[] raiders;

    public Creature GetCreature(int id)
    {
        return GetCreature(creatures, id);
    }

    public Creature GetCitizen(int id)
    {
        return GetCreature(citizens, id);
    }

    public Creature GetRandomCitizen()
    {
        return GetRandomCreature(citizens);
    }

    public Creature GetWanderer(int id)
    {
        return GetCreature(wanderers, id);
    }

    public Creature GetRandomWanderer()
    {
        return GetRandomCreature(wanderers);
    }

    public Creature GetRaider(int id)
    {
        return GetCreature(raiders, id);
    }

    public Creature GetRandomRaider()
    {
        return GetRandomCreature(raiders);
    }

    private Creature GetCreature(Creature[] array, int id)
    {
        if (id >= array.Length) return null;

        return array[id];
    }

    private Creature GetRandomCreature(Creature[] array)
    {
        int id = UnityEngine.Random.Range(0, array.Length);

        return GetCreature(array, id);
    }
}