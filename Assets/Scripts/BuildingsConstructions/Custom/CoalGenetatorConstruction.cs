using UnityEngine;

public class CoalGenetatorConstruction : BuildingConstruction
{
    [SerializeField] private Transform smokeSpawnTransform = null;
    public Transform SmokeSpawnTransform => smokeSpawnTransform;
}
