using UnityEngine;
using UnityEngine.UI;

public class ProgressDisplay : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image progressImage;

    private bool isShown => root.activeSelf;

    public void Show()
    {
        if (isShown) return;

        root.SetActive(true);
    }

    public void Hide()
    {
        if (!isShown) return;

        root.SetActive(false);
    }

    public void SetProgress(float value)
    {
        progressImage.fillAmount = value;
    }
}