using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "boatPrefabsList", menuName = "GameContent/BoatList")]
public class boatsList : ScriptableObject
{
    [field: SerializeField] public List<Boat> boats { get; private set; } = new List<Boat>();
}
