using UnityEngine;

[System.Serializable]
public struct BakedConstruction
{
    [SerializeField] private BakedLevel[] levels;
}

[System.Serializable]
public struct BakedLevel
{
    [SerializeField] private BakedMesh[] placeIndexes;
}

[System.Serializable]
public struct BakedMesh
{
    [SerializeField] private MeshRenderer mesh;
    public MeshRenderer Mesh => mesh;
}

public class BakedConstructionMeshDatabase : MonoBehaviour
{
    [SerializeField] private BakedConstruction[] constructions = { };
    public BakedConstruction[] Constructions => constructions;
}
