using UnityEngine;

public class GPUInstancingEnabler : MonoBehaviour
{
    private static MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        if (meshRenderer == null) {
            Debug.LogError($"Mesh Renderer is not valid at {name}!");
            return;
        }

        if (propertyBlock == null) {
            propertyBlock = new MaterialPropertyBlock();
        }

        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}