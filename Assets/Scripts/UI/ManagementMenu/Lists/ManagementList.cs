using UnityEngine;
using UnityEngine.UI;

public abstract class ManagementList : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup layoutGroup;
    public GridLayoutGroup LayoutGroup => layoutGroup;

    [SerializeField] private ScrollRect scrollRect;
    public ScrollRect ScrollRect => scrollRect;

    private void Start()
    {
        CreateWidgets();
    }

    protected abstract void CreateWidgets();
}