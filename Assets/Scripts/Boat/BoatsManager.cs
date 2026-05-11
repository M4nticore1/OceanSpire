using System.Collections.Generic;
using UnityEngine;

public class BoatsManager : MonoBehaviour
{
    public static BoatsManager Instance { get; private set; }

    private List<Boat> boats = new List<Boat>();
    public IReadOnlyList<Boat> Boats => boats.AsReadOnly();

    private Dictionary<int, Boat> boatsDict = new Dictionary<int, Boat>();

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterBoat(Boat boat)
    {
        boats.Add(boat);
        boatsDict.Add(boat.InstanceId.Id, boat);
    }

    public void UnregisterBoat(Boat boat)
    {
        boats.Remove(boat);
        boatsDict.Remove(boat.InstanceId.Id);
    }

    public Boat GetBoat(int id)
    {
        Boat boat;
        boatsDict.TryGetValue(id, out boat);

        return boat;
    }

    public Boat GetBoatByInteractorIndex(int index)
    {
        if (boats.Count <= index) return null;

        Boat boat = boats[index];
        return boat;
    }
}