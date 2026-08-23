using System;
using System.Collections.Generic;
using UnityEngine;

public class BoatsManager : MonoBehaviour
{
    public static BoatsManager Instance { get; private set; }

    [SerializeField] private List<Boat> boats = new();
    public IReadOnlyList<Boat> Boats => boats;

    [SerializeField] private List<Boat> citizenBoats = new();
    public IReadOnlyList<Boat> CitizenBoats => citizenBoats;

    [SerializeField] private List<Boat> wandererBoats = new();
    public IReadOnlyList<Boat> WandererBoats => wandererBoats;

    [SerializeField] private List<Boat> raiderBoats = new();
    public IReadOnlyList<Boat> RaiderBoats => raiderBoats;

    [SerializeField] private List<Boat> evictBoats = new();
    public IReadOnlyList<Boat> EvictBoats => evictBoats;

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
            case BoatStatusEnum.Citizen:
                RegisterBoat(citizenBoats, boat);
                break;
            case BoatStatusEnum.Wanderer:
                RegisterBoat(wandererBoats, boat);
                break;
            case BoatStatusEnum.Raider:
                RegisterBoat(raiderBoats, boat);
                break;
            case BoatStatusEnum.Evicted:
                RegisterBoat(evictBoats, boat);
                break;
        }
    }

    public void UnregisterBoat(Boat boat)
    {
        switch (boat.CurrentStatus) {
            case BoatStatusEnum.Citizen:
                UnregisterBoat(citizenBoats, boat);
                break;
            case BoatStatusEnum.Wanderer:
                UnregisterBoat(wandererBoats, boat);
                break;
            case BoatStatusEnum.Raider:
                UnregisterBoat(raiderBoats, boat);
                break;
            case BoatStatusEnum.Evicted:
                UnregisterBoat(evictBoats, boat);
                break;
        }
    }

    public Boat GetFirstFreeBoat(IReadOnlyList<Boat> boatsList)
    {
        if (boatsList == null) return null;

        foreach (var boat in boatsList) {
            if (boat.TargetRider != null) continue;
            if (boat.CurrentRider != null) continue;

            return boat;
        }

        return null;
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