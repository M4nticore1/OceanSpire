using System;
using UnityEngine;

public class InstanceId : MonoBehaviour
{
    private Guid guid;
    private bool isRegistered;

    private InstancesManager instancesManager => InstancesManager.Instance;

    private void OnDestroy()
    {
        Unregister();
    }

    public void NewGuid()
    {
        SetGuid(Guid.NewGuid());
    }

    public void SetGuid(Guid newGuid)
    {
        if (newGuid == Guid.Empty) {
            Debug.LogError("New guid to set is empty!");
            return;
        }

        if (newGuid == guid) return;

        if (isRegistered) {
            Unregister();
        }

        guid = newGuid;
        Register();
    }

    private void Register()
    {

        instancesManager.RegisterInstance(this);
        isRegistered = true;
    }

    private void Unregister()
    {
        instancesManager.UnregisterInstance(this);
        isRegistered = false;
    }

    public Guid GetGuid()
    {
        return guid;
    }
}