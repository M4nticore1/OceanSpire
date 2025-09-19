using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ResourceToBuild
{
    public ItemData resourceData;
    public int amount;
}

[CreateAssetMenu(fileName = "BuildingLevelData", menuName = "Scriptable Objects/BuildingLevelData")]
public class BuildingLevelData : ScriptableObject
{
    [Header("Main")]
    public List<ResourceToBuild> ResourcesToBuild = new List<ResourceToBuild>();
    public int maxResidentsCount = 0;
}
