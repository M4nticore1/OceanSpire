using UnityEngine;
using UnityEngine.UI;

public class ProgressDisplay : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] Image progressImage;

    public void Display()
    {
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void SetProgress(float value)
    {
        progressImage.fillAmount = value;
    }
}