using System;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public int Id { get; private set; } = 0;
    public int InstanceId { get; private set; } = 0;
    public int Level { get; private set; } = 1;
    public ConstructionData ConstructionData { get; private set; }

    public BuildingData(int id, int instanceId, int level, ConstructionData constructionData)
    {
        Id = id;
        InstanceId = instanceId;
        Level = level;
        ConstructionData = constructionData;
    }
}
