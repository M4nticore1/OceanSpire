using UnityEngine;

enum CreatureIdEnum
{
    Human,
    Shark
}

[CreateAssetMenu(fileName = "CreatureData", menuName = "Scriptable Objects/CreatureData")]
public class CreatureData : ScriptableObject
{
    [SerializeField] private CreatureIdEnum creatureId = CreatureIdEnum.Human;
    public int CreatureId => (int)creatureId;
}
