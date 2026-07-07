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

    [SerializeField] private Dictionary<CreatureIdEnum, Creature> creaturesDict;

    public Creature GetCreature(CreatureIdEnum id)
    {
        TryInitDict(creatures, ref creaturesDict);

        return GetCreature(creaturesDict, id);
    }

    private Creature GetCreature(Dictionary<CreatureIdEnum, Creature> creatures, CreatureIdEnum id)
    {
        return creatures[id];
    }

    private void TryInitDict(Creature[] creatures, ref Dictionary<CreatureIdEnum, Creature> creaturesDict)
    {
        if (creaturesDict != null) return;

        creaturesDict = new();

        foreach (var creature in creatures) {
            creaturesDict.Add(creature.Definition.CreatureId, creature);
        }
    }
}