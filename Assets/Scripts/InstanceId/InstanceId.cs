using UnityEngine;

public class InstanceId : MonoBehaviour
{
    public int Id { get; private set; } = 0;
    private bool isRegistered = false;

    public void Register(int id)
    {
        if (isRegistered) {
            Debug.Log($"Instance Id component of {this} is already registered as {Id}!");
            return;
        }

        Id = id;
        InstancesManager.Instance.RegisterInstance(id, this);
        isRegistered = true;
    }
}