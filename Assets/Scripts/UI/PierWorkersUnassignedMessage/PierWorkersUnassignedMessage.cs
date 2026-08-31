using TMPro;
using UnityEngine;

public class PierWorkersUnassignedMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float showTime = 5f;
    private float currentShowTime = 0f;

    [SerializeField] private float fadeTime = 2f;
    private float currentFadeTime = 0f;

    public void Tick()
    {
        if (!gameObject.activeSelf) return;
        if (!gameObject.activeInHierarchy) return;

        if (currentShowTime < showTime) {
            currentShowTime += Time.deltaTime;
        }

        if (currentShowTime >= showTime) {
            if (currentFadeTime < fadeTime) {
                ProcessFade();
            }
            else {
                Hide();
            }
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        text.alpha = 1f;
        currentShowTime = 0f;
        currentFadeTime = 0f;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ProcessFade()
    {
        currentFadeTime += Time.deltaTime;

        var alpha = 1f - Mathf.Lerp(0f, 1f, currentFadeTime / fadeTime);
        alpha = Mathf.Clamp01(alpha);

        text.alpha = alpha;
    }
}