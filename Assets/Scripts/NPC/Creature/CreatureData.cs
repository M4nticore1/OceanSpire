using System;
using UnityEngine;

[Serializable]
public class CreatureData
{
    public CreatureIdEnum Id = 0;
    public Guid InstanceId = Guid.NewGuid();
    public Vector3Data Position = Vector3Data.Zero();
    public Vector3Data Rotation = Vector3Data.Zero();
    public HealthData Health = HealthData.Default();

    public static CreatureData Create(Creature creature)
    {
        if (creature == null) return null;

        var creatureData = new CreatureData();
        creatureData.FillCreatureData(creature);

        return creatureData;
    }

    protected void FillCreatureData(Creature creature)
    {
        if (creature == null) return;

        Id = creature.Definition.CreatureId;
        InstanceId = creature.InstanceId.GetGuid();
        Position = new Vector3Data(creature.transform.position);
        Rotation = new Vector3Data(creature.transform.rotation.eulerAngles);
        Health = HealthData.Create(creature.HealthComponent);
    }
}