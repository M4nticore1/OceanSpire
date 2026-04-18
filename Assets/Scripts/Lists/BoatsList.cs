using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "boatPrefabsList", menuName = "GameContent/BoatList")]
public class BoatsList : ScriptableObject
{
    private static BoatsList _instance;
    public static BoatsList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<BoatsList>("Lists/BoatsList");
            }
            return _instance;
        }
    }

    [field: SerializeField] public List<Boat> boats { get; private set; } = new List<Boat>();
}
