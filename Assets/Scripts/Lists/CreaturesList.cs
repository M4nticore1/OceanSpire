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
            }

            return instance;
        }
    }

    [SerializeField] private Creature[] creatures;
    public IReadOnlyList<Creature> Creatures => creatures;

    [SerializeField] private Dictionary<int, Creature> creaturesDict;

    [SerializeField] private Creature[] citizens;
    public IReadOnlyList<Creature> Citizens => citizens;

    [SerializeField] private Dictionary<int, Creature> citizensDict;

    [SerializeField] private Creature[] wanderers;
    public IReadOnlyList<Creature> Wanderers => wanderers;

    [SerializeField] private Dictionary<int, Creature> wanderersDict;

    [SerializeField] private Creature[] raiders;
    public IReadOnlyList<Creature> Raiders => raiders;

    [SerializeField] private Dictionary<int, Creature> raidersDict;

    public Creature GetCreature(int id)
    {
        TryInitDict(creatures, ref creaturesDict);

        return GetCreature(creaturesDict, id);
    }

    public Creature GetCitizen(int id)
    {
        TryInitDict(citizens, ref citizensDict);

        return GetCreature(citizensDict, id);
    }

    public Creature GetWanderer(int id)
    {
        TryInitDict(wanderers, ref wanderersDict);

        return GetCreature(wanderersDict, id);
    }

    public Creature GetRaider(int id)
    {
        TryInitDict(raiders, ref raidersDict);

        return GetCreature(raidersDict, id);
    }

    public Creature GetRandomCitizen()
    {
        return GetRandomCreature(citizens);
    }

    public Creature GetRandomWanderer()
    {
        return GetRandomCreature(wanderers);
    }

    public Creature GetRandomRaider()
    {
        return GetRandomCreature(raiders);
    }

    private Creature GetCreature(Dictionary<int, Creature> creatures, int id)
    {
        return creatures[id];
    }

    private Creature GetRandomCreature(Creature[] creatures)
    {
        int index = UnityEngine.Random.Range(0, creatures.Length);

        return creatures[index];
    }

    private void TryInitDict(Creature[] creatures, ref Dictionary<int, Creature> creaturesDict)
    {
        if (creaturesDict != null) return;

        creaturesDict = new();

        foreach (var creature in creatures) {
            creaturesDict.Add(creature.Definition.CreatureId, creature);
        }
    }
}