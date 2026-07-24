using System;
using UnityEngine;

public class InstanceId : MonoBehaviour
{
    private Guid guid = Guid.Empty;
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
            Debug.LogError($"[{nameof(InstanceId)}] New guid you want to set is empty at {name}!");
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