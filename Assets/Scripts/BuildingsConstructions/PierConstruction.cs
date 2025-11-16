using UnityEngine;
using System.Collections.Generic;

public class PierConstruction : BuildingConstruction
{
    [SerializeField] private List<Transform> boatDockPositions = new List<Transform>();
    public List<Transform> BoatDockPositions { get { return new List<Transform>(boatDockPositions); } }
}
