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

    public void RegisterBoat(Boat boat)
    {
        switch (boat.CurrentStatus) {
            case HumanStatusEnum.Citizen:
                RegisterBoat(citizenBoats, boat);
                RegisterBoat(citizenBoatsDict, boat);
                break;
            case HumanStatusEnum.Wanderer:
                RegisterBoat(wandererBoats, boat);
                RegisterBoat(wandererBoatsDict, boat);
                break;
            case HumanStatusEnum.Raider:
                RegisterBoat(raiderBoats, boat);
                RegisterBoat(raiderBoatsDict, boat);
                break;
        }
    }

    public void UnregisterBoat(Boat boat)
    {
        switch (boat.CurrentStatus) {
            case HumanStatusEnum.Citizen:
                UnregisterBoat(citizenBoats, boat);
                UnregisterBoat(citizenBoatsDict, boat);
                break;
            case HumanStatusEnum.Wanderer:
                UnregisterBoat(wandererBoats, boat);
                UnregisterBoat(wandererBoatsDict, boat);
                break;
            case HumanStatusEnum.Raider:
                UnregisterBoat(raiderBoats, boat);
                UnregisterBoat(raiderBoatsDict, boat);
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
        var guid = boat.InstanceId.GetGuid();
        if (!boatsDict.ContainsKey(guid)) {
            boatsDict.Add(guid, boat);
        }

        if (!this.boatsDict.ContainsKey(guid)) {
            this.boatsDict.Add(guid, boat);
        }
    }

    private void UnregisterBoat(Dictionary<Guid, Boat> boatsDict, Boat boat)
    {
        var guid = boat.InstanceId.GetGuid();
        if (!boatsDict.ContainsKey(guid)) return;

        boatsDict.Remove(guid);
        this.boatsDict.Remove(guid);
    }

    private void RegisterBoat(List<Boat> boatsList, Boat boat)
    {
        if (!boatsList.Contains(boat)) {
            boatsList.Add(boat);
        }

        if (!boats.Contains(boat)) {
            boats.Add(boat);
        }
    }

    private void UnregisterBoat(List<Boat> boatsList, Boat boat)
    {
        if (boatsList.Contains(boat)) {
            boatsList.Remove(boat);
        }

        if (boats.Contains(boat)) {
            boats.Remove(boat);
        }
    }
}