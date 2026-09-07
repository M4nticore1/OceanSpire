using UnityEngine;
using UnityEngine.UI;

public abstract class ManagementList : MonoBehaviour
{
    [SerializeField] private LayoutGroup layoutGroup;
    public LayoutGroup LayoutGroup => layoutGroup;

    [SerializeField] private ScrollRect scrollRect;
    public ScrollRect ScrollRect => scrollRect;

    protected virtual void Start()
    {
        CreateWidgets();
    }

    protected abstract void CreateWidgets();
}