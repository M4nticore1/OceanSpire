using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Texture texture;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int BaseMapProp = Shader.PropertyToID("_BaseMap");

    private void Start()
    {
        propertyBlock = new MaterialPropertyBlock();

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(BaseMapProp, texture);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}