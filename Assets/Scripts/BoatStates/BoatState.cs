using System;
using UnityEngine;

public abstract class BoatState
{
    protected Boat boat = null;

    public static event Action<Boat, BoatState> OnStateEntered;
    public static event Action<Boat, BoatState> OnStateExited;

    public BoatState(Boat boat)
    {
        this.boat = boat;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Tick();
    public abstract void OnReachedPath();
    public abstract void OnBoatDockChanged(BoatDockPoint boatDock);
}