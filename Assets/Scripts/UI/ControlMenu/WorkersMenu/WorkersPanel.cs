using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkersPanel : UIBehaviour
{
    private RectTransform rectTransform;

    [SerializeField] private GridLayoutGroup layoutGroup;
    public GridLayoutGroup LayoutGroup => layoutGroup;

    [SerializeField] private GameObject haveNoCitizensText;

    private Vector2 startSize;

    protected override void Awake()
    {
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
        startSize = rectTransform.sizeDelta;
    }

    public void UpdateMenu()
    {
        if (layoutGroup.transform.childCount == 0) {
            rectTransform.sizeDelta = startSize;
        }
        else {
            layoutGroup.GetComponent<RectTransform>().ForceUpdateRectTransforms();
            float ySize = startSize.y + (LayoutGroupUtils.GetRowsCount(layoutGroup) * layoutGroup.cellSize.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ySize);
        }

        if (haveNoCitizensText) {
            haveNoCitizensText.SetActive(layoutGroup.transform.childCount == 0);
        }
    }
}