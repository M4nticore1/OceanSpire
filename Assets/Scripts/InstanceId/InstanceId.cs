using UnityEngine;

public class InstanceId : MonoBehaviour
{
    public int id { get; private set; } = 0;
    private bool isRegistered = false;

    public void Init()
    {
        if (isRegistered) {
            Debug.Log($"Instance Id is already registered as {this.id}!");
            return;
        }

        int id = InstancesManager.instance.GetNextInstanceId();
        this.id = id;
        InstancesManager.instance.AddInstanceId(this.id);

        isRegistered = true;
    }

    public void Init(int id)
    {
        if (isRegistered) {
            Debug.Log($"Instance Id is already registered as {this.id}!");
            return;
        }

        this.id = id;
        InstancesManager.instance.AddInstanceId(this.id);

        isRegistered = true;
    }
}