using UnityEngine;

enum CreatureIdEnum
{
    Human,
    Shark
}

[CreateAssetMenu(fileName = "CreatureDefinition", menuName = "Creature/CreatureDefinition")]
public class CreatureDefinition : ScriptableObject
{
    [SerializeField] private CreatureIdEnum creatureId = CreatureIdEnum.Human;
    public int CreatureId => (int)creatureId;
}
