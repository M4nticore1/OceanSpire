using UnityEngine;
using UnityEngine.UI;

public class BuildingIndicator : MonoBehaviour
{
    [SerializeField] private BuildingConstruction buildingConstruction;
    public BuildingConstruction BuildingConstruction => buildingConstruction;

    [SerializeField] private Texture indicatorTexture;
    public Texture IndicatorTexture => indicatorTexture;

    [SerializeField] private MaterialController materialController;
    [SerializeField] private GameObject content;
    [SerializeField] private Collider collision;
    [SerializeField] private LayoutElement layoutElement;

    private bool isShown => content.activeSelf;

    private void Start()
    {
        UpdateLayoutElementIgnored();
    }

    public void Show(Texture indicatorTexture)
    {
        Debug.Log("Show");
        content.SetActive(true);
        collision.enabled = true;
        UpdateLayoutElementIgnored();

        materialController.SetBaseMap(indicatorTexture);
    }

    public void Hide()
    {
        Debug.Log("Hide");
        content.SetActive(false);
        collision.enabled = false;
        UpdateLayoutElementIgnored();
    }

    private void UpdateLayoutElementIgnored()
    {
        if (layoutElement == null) return;

        layoutElement.ignoreLayout = !isShown;
    }
}