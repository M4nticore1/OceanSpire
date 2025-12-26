using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoatPrefabsList", menuName = "GameContent/BoatList")]
public class BoatPrefabsList : ScriptableObject
{
    [SerializeField] private List<Boat> boatPrefabs = new List<Boat>();
    public List<Boat> BoatPrefabs => boatPrefabs;
}
