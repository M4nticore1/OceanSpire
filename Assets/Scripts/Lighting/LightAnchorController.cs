using UnityEngine;

public class LightAnchorController : MonoBehaviour
{
    private void Start()
    {
        var manager = FindAnyObjectByType<LightProbeGroupManager>();
        if (manager != null) {
            var anchor = manager.ProbeAnchor;
            var renderer = GetComponent<MeshRenderer>();
            renderer.probeAnchor = anchor;
        }
    }
}
