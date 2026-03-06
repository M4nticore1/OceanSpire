using UnityEngine;

public class LightAnchorController : MonoBehaviour
{
    private void Start()
    {
        LightProbeGroupManager manager = FindAnyObjectByType<LightProbeGroupManager>();
        Transform anchor = manager.ProbeAnchor;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.probeAnchor = anchor;
    }
}
