using System;
using System.Collections.Generic;
using UnityEngine;

public class BoatsManager : MonoBehaviour
{
    public static BoatsManager Instance { get; private set; }

    [SerializeField] private List<Boat> boats = new();
    public IReadOnlyList<Boat> Boats => boats;

    private Dictionary<Guid, Boat> boatsDict = new();
    public IReadOnlyDictionary<Guid, Boat> BoatsDict => boatsDict;

    [SerializeField] private List<Boat> citizenBoats = new();
    public IReadOnlyList<Boat> CitizenBoats => citizenBoats;

    private Dictionary<Guid, Boat> citizenBoatsDict = new();
    public IReadOnlyDictionary<Guid, Boat> CitizenBoatsDict => citizenBoatsDict;

    [SerializeField] private List<Boat> wandererBoats = new();
    public IReadOnlyList<Boat> WandererBoats => wandererBoats;

    private Dictionary<Guid, Boat> wandererBoatsDict = new();
    public IReadOnlyDictionary<Guid, Boat> WandererBoatsDict => wandererBoatsDict;

    [SerializeField] private List<Boat> raiderBoats = new();
    public IReadOnlyList<Boat> RaiderBoats => raiderBoats;

    private Dictionary<Guid, Boat> raiderBoatsDict = new();
    public IReadOnlyDictionary<Guid, Boat> RaiderBoatsDict => raiderBoatsDict;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        foreach (var boat in boats) {
            boat.Tick();
        }
    }

    public void RegisterCitizenBoat(Boat boat)
    {
        RegisterBoat(citizenBoats, boat);
        RegisterBoat(citizenBoatsDict, boat);
    }

    public void UnregisterCitizenBoat(Boat boat)
    {
        UnregisterBoat(citizenBoats, boat);
        UnregisterBoat(citizenBoatsDict, boat);
    }

    public void RegisterWandererBoat(Boat boat)
    {
        RegisterBoat(wandererBoats, boat);
        RegisterBoat(wandererBoatsDict, boat);
    }

    public void UnregisterWandererBoat(Boat boat)
    {
        UnregisterBoat(wandererBoats, boat);
        UnregisterBoat(wandererBoatsDict, boat);
    }

    public void RegisterRaiderBoat(Boat boat)
    {
        RegisterBoat(raiderBoats, boat);
        RegisterBoat(raiderBoatsDict, boat);
    }

    public void UnregisterRaiderBoat(Boat boat)
    {
        UnregisterBoat(raiderBoats, boat);
        UnregisterBoat(raiderBoatsDict, boat);
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

    public Boat GetBoat(Guid id)
    {
        boatsDict.TryGetValue(id, out var boat);

        return boat;
    }

    private void RegisterBoat(Dictionary<Guid, Boat> boatsDict, Boat boat)
    {
        boatsDict.Add(boat.InstanceId.GetGuid(), boat);
        this.boatsDict.Add(boat.InstanceId.GetGuid(), boat);
    }

    private void UnregisterBoat(Dictionary<Guid, Boat> boatsDict, Boat boat)
    {
        boatsDict.Remove(boat.InstanceId.GetGuid());
        this.boatsDict.Remove(boat.InstanceId.GetGuid());
    }

    private void RegisterBoat(List<Boat> boatsList, Boat boat)
    {
        boatsList.Add(boat);
        boats.Add(boat);
    }

    private void UnregisterBoat(List<Boat> boatsList, Boat boat)
    {
        boatsList.Remove(boat);
        boats.Remove(boat);
    }
}