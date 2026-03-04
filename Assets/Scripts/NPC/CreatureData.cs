using UnityEngine;

enum CreatureIdEnum
{
    Citizen,
    Shark
}

[CreateAssetMenu(fileName = "CreatureData", menuName = "Scriptable Objects/CreatureData")]
public class CreatureData : ScriptableObject
{
    [SerializeField] private CreatureIdEnum creatureId = CreatureIdEnum.Citizen;
    public int CreatureId => (int)creatureId;
}
