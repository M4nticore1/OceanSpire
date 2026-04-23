using UnityEngine;

public class ConstructionData
{
    public float ConstructionTime { get; private set; }
    public bool UnderConstruction { get; private set; }

    public ConstructionData(float constructionTime, bool underConstruction)
    {
        ConstructionTime = constructionTime;
        UnderConstruction = underConstruction;
    }
}
