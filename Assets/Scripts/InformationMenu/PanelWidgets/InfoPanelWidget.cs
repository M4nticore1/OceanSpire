using UnityEngine;

public abstract class InfoPanelWidget : MonoBehaviour
{
    protected virtual void HandleShow(InfoPanelData sectionData)
    {

    }

    public void Show(InfoPanelData panelData)
    {
        if (panelData == null) return;

        gameObject.SetActive(true);
        HandleShow(panelData);
    }

    public void Hide()
    {
        //var sizeDelta = rectTransform.sizeDelta;
        //rectTransform.sizeDelta = new Vector2(sizeDelta.x, 0);
        gameObject.SetActive(false);
    }
}