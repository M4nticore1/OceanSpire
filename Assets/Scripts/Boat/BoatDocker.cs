using System;
using UnityEngine;

public class BoatDocker : MonoBehaviour
{
    Movement movement = null;

    public bool isDocked { get; private set; } = true;
    public bool isReturningToDock { get; private set; } = false;

    public BoatDockPoint dockPoint { get; private set; } = null;

    public static event Action<BoatDocker> onBoatDocked;

    private void Awake()
    {
        movement = GetComponent<Movement>();
    }

    public void OnEnteredBoat()
    {
        ExitDock();
    }

    public void OnReachedPath()
    {
        EnterDock();
    }

    public void SetDock(BoatDockPoint dockPoint)
    {
        this.dockPoint = dockPoint;
    }

    public void HandleDocking()
    {
        ProcessMooring();
    }

    public void HandleCollectedLoot()
    {
        StartMovingToDock();
    }

    private void EnterDock()
    {
        Debug.Log("EnterDock");
        isDocked = true;
        isReturningToDock = false;
        onBoatDocked?.Invoke(this);
    }

    private void ExitDock()
    {
        isDocked = false;
    }

    private void StartMovingToDock()
    {
        movement.TryMoveTo(dockPoint.DockTransform.position);
        isReturningToDock = true;
    }

    private void ProcessMooring()
    {
        if (transform.rotation == dockPoint.DockTransform.rotation) return;

        transform.rotation = Quaternion.Lerp(transform.rotation, dockPoint.DockTransform.rotation, BoatData.correctDockRotationSpeed * Time.deltaTime);
    }
}
