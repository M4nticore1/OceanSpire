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
                instance.Init();
            }

            return instance;
        }
    }

    [SerializeField] private List<Boat> boats = new();
    private Dictionary<int, Boat> boatsDict = new();

    private void Init()
    {
        foreach (var boat in boats) {
            boatsDict.Add(boat.BoatData.BoatId, boat);
        }
    }

    public Boat GetBoat(int id)
    {
        Boat boat = null;
        boatsDict.TryGetValue(id, out boat);

        return boat;
    }
}
