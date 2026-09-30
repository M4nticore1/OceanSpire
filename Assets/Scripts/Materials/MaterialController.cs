using UnityEngine;

public class MaterialController : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int BaseMapProp = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetBaseMap(Texture texture)
    {
        if (meshRenderer == null) {
            Debug.LogError($"[{nameof(MaterialController)}] MeshRenderer is not valid!");
            return;
        }
        if (texture == null) {
            Debug.LogError($"[{nameof(MaterialController)}] Texture is not valid!");
        }

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(BaseMapProp, texture);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}