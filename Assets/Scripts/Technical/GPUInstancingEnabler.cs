using UnityEngine;

public class GPUInstancingEnabler : MonoBehaviour
{
    private void Awake()
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (!meshRenderer)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer)
            meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
