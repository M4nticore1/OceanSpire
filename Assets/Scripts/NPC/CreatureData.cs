using UnityEngine;

enum CreatureIdEnum
{
    Resident,
    Shark
}

[CreateAssetMenu(fileName = "CreatureData", menuName = "Scriptable Objects/CreatureData")]
public class CreatureData : ScriptableObject
{
    [SerializeField] private CreatureIdEnum creatureId = CreatureIdEnum.Resident;
    public int CreatureId => (int)creatureId;
}
