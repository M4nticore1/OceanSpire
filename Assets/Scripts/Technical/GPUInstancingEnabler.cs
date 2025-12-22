using UnityEngine;

public class GPUInstancingEnabler : MonoBehaviour
{
    private void Awake()
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
