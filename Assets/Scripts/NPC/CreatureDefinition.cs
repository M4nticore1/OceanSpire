using UnityEngine;

enum CreatureIdEnum
{
    HumanCitizenMale,
    HumanCitizenFemale,
    HumanWandererMale,
    HumanWandererFemale,
    HumanRaiderMale,
    HumanRaiderFemale,
}

[CreateAssetMenu(fileName = "CreatureDefinition", menuName = "Creature/CreatureDefinition")]
public class CreatureDefinition : ScriptableObject
{
    [SerializeField] private CreatureIdEnum creatureId = CreatureIdEnum.HumanCitizenMale;
    public int CreatureId => (int)creatureId;
}
