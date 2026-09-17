using UnityEngine;

public abstract class InfoPanelWidget : MonoBehaviour
{
    public InfoPanelData InfoPanelData { get; private set; }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void HandleShow(InfoPanelData infoPanelData)
    {

    }

    public void Show(InfoPanelData infoPanelData)
    {
        if (infoPanelData == null) {
            Debug.LogError($"[{nameof(InfoPanelWidget)}] Info Panel Widget Data not valid at {gameObject}!");
            return;
        }

        InfoPanelData = infoPanelData;

        gameObject.SetActive(true);
        HandleShow(infoPanelData);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}