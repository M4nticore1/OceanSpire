using UnityEngine;

public class GPUInstancingEnabler : MonoBehaviour
{
    private void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();

        if (!meshRenderer) {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        if (!meshRenderer) {
            Debug.LogError($"meshRenderer not fount at {name}");
            return;
        }

        var propertyBlock = new MaterialPropertyBlock();
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}