using System.Collections.Generic;
using UnityEngine;

public class BoatsManager : MonoBehaviour
{
    public static BoatsManager Instance { get; private set; } = null;

    public List<Boat> boats { get; private set; } = new List<Boat>();
    public Dictionary<int, Boat> boatsDict { get; private set; } = new Dictionary<int, Boat>();

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (Boat boat in boats) {
            boatsDict.Add(boat.instanceId, boat);
        }
    }

    public void RegisterBoat(Boat boat)
    {
        boats.Add(boat);
        boatsDict.Add(boat.instanceId, boat);
    }

    public void UnregisterBoat(Boat boat)
    {
        boats.Remove(boat);
        boatsDict.Remove(boat.instanceId);
    }

    public Boat GetBoatByInteractorIndex(int index)
    {
        if (boats.Count <= index) return null;

        Boat boat = boats[index];
        return boat;
    }
}