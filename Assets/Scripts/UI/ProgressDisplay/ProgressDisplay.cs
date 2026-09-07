using UnityEngine;
using UnityEngine.UI;

public class ProgressDisplay : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image progressImage;
    [SerializeField] private LayoutElement layoutElement;

    private bool isShown => root.activeSelf;

    private void Start()
    {
        UpdateIgnoreLayout();
    }

    public void Show()
    {
        if (isShown) return;

        root.SetActive(true);
        UpdateIgnoreLayout();
    }

    public void Hide()
    {
        if (!isShown) return;

        root.SetActive(false);
        UpdateIgnoreLayout();
    }

    public void SetProgress(float value)
    {
        progressImage.fillAmount = value;
    }

    private void UpdateIgnoreLayout()
    {
        if (layoutElement != null) {
            layoutElement.ignoreLayout = !isShown;
        }
    }
}