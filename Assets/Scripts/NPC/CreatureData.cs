using UnityEngine;

public abstract class CreatureDataV1
{
    public int instanceId { get; private set; } = -1;
    public int id { get; private set; }
    public Vector3 position { get; private set; }
    public Vector3 rotation { get; private set; }

    public CreatureDataV1(int id, int instanceId, Vector3 position, Vector3 rotation)
    {
        this.id = id;
        this.instanceId = instanceId;
        this.position = position;
        this.rotation = rotation;
    }

    public void SetPosition(Vector3 value)
    {
        position = value;
    }

    public void SetRotation(Vector3 value)
    {
        rotation = value;
    }
}