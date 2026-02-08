using UnityEngine;

public class BoatDockPoint : MonoBehaviour
{
    [SerializeField] private Transform dockTransform = null;
    public Transform DockTransform => dockTransform;
    [SerializeField] private Transform entranceTransform = null;
    public Transform EntraceTransform => entranceTransform;
}
