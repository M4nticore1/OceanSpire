using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkersPanel : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private GridLayoutGroup layoutGroup;
    public GridLayoutGroup LayoutGroup => layoutGroup;

    [SerializeField] private GameObject haveNoCitizensText;

    private Vector2? startSize = null;

    private void Awake()
    {
        startSize = rectTransform.sizeDelta;
    }

    private void Start()
    {
        UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (startSize == null) return;

        if (layoutGroup.transform.childCount == 0) {
            rectTransform.sizeDelta = startSize.Value;
        }
        else {
            layoutGroup.GetComponent<RectTransform>().ForceUpdateRectTransforms();
            float ySize = startSize.Value.y + (LayoutGroupUtils.GetRowsCount(layoutGroup) * layoutGroup.cellSize.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ySize);
        }

        if (haveNoCitizensText) {
            haveNoCitizensText.SetActive(layoutGroup.transform.childCount == 0);
        }
    }
}