using UnityEngine;

public class BoatDockPoint : MonoBehaviour
{
    public Boat boat { get; private set; }

    [SerializeField] private Transform dockTransform = null;
    public Transform DockTransform => dockTransform;
    [SerializeField] private Transform entranceTransform = null;
    public Transform EntraceTransform => entranceTransform;

    public void SetBoat(Boat boat)
    {
        this.boat = boat;
    }
}