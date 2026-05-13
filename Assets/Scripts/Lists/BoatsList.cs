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

    [SerializeField] private List<Boat> boats = new();

    private Dictionary<int, Boat> boatsDict;

    private Dictionary<int, Boat> BoatsDict
    {
        get
        {
            if (boatsDict == null) {
                boatsDict = new();

                foreach (var boat in boats) {
                    boatsDict.Add(boat.BoatData.BoatId, boat);
                }
            }

            return boatsDict;
        }
    }

    public Boat GetBoat(int id)
    {
        Boat boat = null;
        BoatsDict.TryGetValue(id, out boat);

        return boat;
    }
}
