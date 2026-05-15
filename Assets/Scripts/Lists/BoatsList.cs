using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "boatPrefabsList", menuName = "GameContent/BoatList")]
public class BoatsList : ScriptableObject
{
    private static BoatsList instance;
    public static BoatsList Instance
    {
        get
        {
            if (instance == null) {
                instance = Resources.Load<BoatsList>("Lists/BoatsList");
            }

            return instance;
        }
    }

    [SerializeField] private Boat[] boats;

    private Dictionary<int, Boat> boatsDict;

    public Boat GetBoat(int id)
    {
        TryInitDictionary(boats, ref boatsDict);

        boatsDict.TryGetValue(id, out var boat);

        return boat;
    }

    private void TryInitDictionary(Boat[] boats, ref Dictionary<int, Boat> boatsDict)
    {
        if (boatsDict != null) return;

        boatsDict = new();

        foreach (var boat in boats) {
            boatsDict.Add(boat.Definition.BoatId, boat);
        }
    }
}