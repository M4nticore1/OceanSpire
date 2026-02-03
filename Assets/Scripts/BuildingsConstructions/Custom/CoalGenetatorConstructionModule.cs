using UnityEngine;

public class CoalGenetatorConstructionModule : MonoBehaviour
{
    [SerializeField] private Transform smokeSpawnTransform = null;
    public Transform SmokeSpawnTransform => smokeSpawnTransform;
}
