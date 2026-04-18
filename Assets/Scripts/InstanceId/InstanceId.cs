using UnityEngine;

public class InstanceId : MonoBehaviour
{
    public int id { get; private set; } = 0;
    private bool isRegistered = false;

    public void Init(int id)
    {
        if (isRegistered) {
            Debug.Log($"Instance Id is already registered as {this.id}!");
            return;
        }

        this.id = id;
        InstancesManager.instance.RegisterInstance(this, id);
        isRegistered = true;
    }
}