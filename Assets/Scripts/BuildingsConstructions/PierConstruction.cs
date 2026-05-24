using UnityEngine;
using System.Collections.Generic;

public class PierConstruction : BuildingConstruction
{
    [SerializeField] private List<BoatDockPoint> boatDocks = new List<BoatDockPoint>();
    public List<BoatDockPoint> BoatDocks { get { return new List<BoatDockPoint>(boatDocks); } }
}