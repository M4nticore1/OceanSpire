using UnityEngine;

public class InstanceId : MonoBehaviour
{
    private int id = 0;
    public bool IsRegistered { get; private set; } = false;

    public void Register(int id)
    {
        if (IsRegistered) {
            Debug.Log($"Instance Id component of {this} is already registered as {this.id}!");
            return;
        }

        this.id = id;
        InstancesManager.Instance.RegisterInstance(id, this);
        IsRegistered = true;
    }

    public int GetInstanceId()
    {
        if (!IsRegistered) {
            Debug.Log("You are trying to get an unregistered InstanceId");
        }

        return id;
    }
}