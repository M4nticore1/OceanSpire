using UnityEngine;

public class FakeLightManager : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer = null;
    private MaterialPropertyBlock propertyBlock = null;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        //SetLightMultiplier(0);
    }

    public void SetLightMultiplier(float value)
    {
        propertyBlock.SetFloat("_LightMultiplier", value);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public void SetFlickingMultiplier(float value)
    {
        propertyBlock.SetFloat("_FlickingMultiplier", value);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public void SetColor(Color value)
    {
        propertyBlock.SetColor("_BaseColor", value);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
