using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CreaturesList", menuName = "GameContent/Creatures List")]
public class CreaturesList : ScriptableObject
{
    private static CreaturesList instance;
    public static CreaturesList Instance
    {
        get
        {
            if (instance == null) {
                instance = Resources.Load<CreaturesList>("Lists/CreaturesList");
                instance.Init();
            }
            return instance;
        }
    }

    [SerializeField] private Creature[] creatures;
    [SerializeField] private Dictionary<int, Creature> creaturesDict = new();

    [SerializeField] private Creature[] citizens;
    [SerializeField] private Dictionary<int, Creature> citizensDict = new();

    [SerializeField] private Creature[] wanderers;
    [SerializeField] private Dictionary<int, Creature> wanderersDict = new();

    [SerializeField] private Creature[] raiders;
    [SerializeField] private Dictionary<int, Creature> raidersDict = new();

    private void Init()
    {
        InitDict(creatures, creaturesDict);
        InitDict(citizens, citizensDict);
        InitDict(wanderers, wanderersDict);
        InitDict(raiders, raidersDict);
    }

    public Creature GetCreature(int id)
    {
        return GetCreature(creaturesDict, id);
    }

    public Creature GetCitizen(int id)
    {
        return GetCreature(citizensDict, id);
    }

    public Creature GetRandomCitizen()
    {
        return GetRandomCreature(citizensDict);
    }

    public Creature GetWanderer(int id)
    {
        return GetCreature(wanderersDict, id);
    }

    public Creature GetRandomWanderer()
    {
        return GetRandomCreature(wanderersDict);
    }

    public Creature GetRaider(int id)
    {
        return GetCreature(raidersDict, id);
    }

    public Creature GetRandomRaider()
    {
        return GetRandomCreature(raidersDict);
    }

    private Creature GetCreature(Dictionary<int, Creature> array, int id)
    {
        Creature creature;
        array.TryGetValue(id, out creature);

        return creature;
    }

    private Creature GetRandomCreature(Dictionary<int, Creature> array)
    {
        int index = UnityEngine.Random.Range(0, array.Values.Count);
        int id = array.Values.ToArray()[index].Definition.CreatureId;

        return GetCreature(array, id);
    }

    private void InitDict(Creature[] creatures, Dictionary<int, Creature> creaturesDict)
    {
        foreach (var creature in creatures) {
            creaturesDict.Add(creature.Definition.CreatureId, creature);
        }
    }
}