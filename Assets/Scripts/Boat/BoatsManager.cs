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
                break;
            case HumanStatusEnum.Wanderer:
                RegisterBoat(wandererBoats, boat);
                break;
            case HumanStatusEnum.Raider:
                RegisterBoat(raiderBoats, boat);
                break;
        }
    }

    public void UnregisterBoat(Boat boat)
    {
        switch (boat.CurrentStatus) {
            case HumanStatusEnum.Citizen:
                UnregisterBoat(citizenBoats, boat);
                break;
            case HumanStatusEnum.Wanderer:
                UnregisterBoat(wandererBoats, boat);
                break;
            case HumanStatusEnum.Raider:
                UnregisterBoat(raiderBoats, boat);
                break;
        }
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