using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum BuildingPosition
{
    Straight,
    Corner
}

[AddComponentMenu("")]
public class TowerBuilding : Building
{
    public TowerBuilding leftConnectedBuilding = null;
    public TowerBuilding rightConnectedBuilding = null;
    public TowerBuilding aboveConnectedBuilding = null;
    public TowerBuilding belowConnectedBuilding = null;
}
