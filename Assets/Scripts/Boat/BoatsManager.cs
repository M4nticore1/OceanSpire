using System.Collections.Generic;
using UnityEngine;

public class BoatsManager : MonoBehaviour
{
    public static BoatsManager Instance { get; private set; }

    private Dictionary<int, Boat> boats = new();
    public IReadOnlyDictionary<int, Boat> Boats => boats;

    private Dictionary<int, Boat> citizenBoats = new();
    public IReadOnlyDictionary<int, Boat> CitizenBoats => citizenBoats;

    private Dictionary<int, Boat> wandererBoats = new();
    public IReadOnlyDictionary<int, Boat> WandererBoats => wandererBoats;

    private Dictionary<int, Boat> raiderBoats = new();
    public IReadOnlyDictionary<int, Boat> RaiderBoats => raiderBoats;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterCitizenBoat(Boat boat)
    {
        RegisterBoat(citizenBoats, boat);
    }

    public void UnregisterCitizenBoat(Boat boat)
    {
        UnregisterBoat(citizenBoats, boat);
    }

    public void RegisterWandererBoat(Boat boat)
    {
        RegisterBoat(wandererBoats, boat);
    }

    public void UnregisterWandererBoat(Boat boat)
    {
        UnregisterBoat(wandererBoats, boat);
    }

    public void RegisterRaiderBoat(Boat boat)
    {
        RegisterBoat(raiderBoats, boat);
    }

    public void UnregisterRaiderBoat(Boat boat)
    {
        UnregisterBoat(raiderBoats, boat);
    }

    public void RegisterBoat(Boat boat)
    {
        switch (boat.CurrentStatus) {
            case HumanStatusEnum.Citizen:
                RegisterCitizenBoat(boat);
                break;
            case HumanStatusEnum.Wanderer:
                RegisterWandererBoat(boat);
                break;
            case HumanStatusEnum.Raider:
                RegisterRaiderBoat(boat);
                break;
        }
    }

    public void UnregisterBoat(Boat boat)
    {
        switch (boat.CurrentStatus) {
            case HumanStatusEnum.Citizen:
                UnregisterCitizenBoat(boat);
                break;
            case HumanStatusEnum.Wanderer:
                UnregisterWandererBoat(boat);
                break;
            case HumanStatusEnum.Raider:
                UnregisterRaiderBoat(boat);
                break;
        }
    }

    public Boat GetBoat(int id)
    {
        boats.TryGetValue(id, out var boat);

        return boat;
    }

    private void RegisterBoat(Dictionary<int, Boat> boatsList, Boat boat)
    {
        boatsList.Add(boat.InstanceId.GetId(), boat);
        boats.Add(boat.InstanceId.GetId(), boat);
    }

    private void UnregisterBoat(Dictionary<int, Boat> boatsList, Boat boat)
    {
        boatsList.Remove(boat.InstanceId.GetId());
        boats.Remove(boat.InstanceId.GetId());
    }
}