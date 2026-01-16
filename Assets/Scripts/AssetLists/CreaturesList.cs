using UnityEngine;

[CreateAssetMenu(fileName = "CreaturesList", menuName = "GameContent/CreaturesList")]
public class CreaturesList : ScriptableObject
{
    [field: SerializeField] public Creature resident { get; private set; } = null;
}
