using UnityEngine;
using UnityEngine.UI;

public class FlickingImage : MonoBehaviour
{
    [SerializeField] private Gradient gradient;
    [SerializeField] private float flickingSpeed = 1;
    [SerializeField] private bool isFlicking = false;
    private Image image;

    private float flickingAlpha = 0f;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (!isFlicking) return;

        flickingAlpha = Mathf.PingPong(flickingSpeed * Time.time, 1f);
        Color color = gradient.Evaluate(flickingAlpha);
        image.color = color;
    }

    public void SetFlickingEnabled(bool value)
    {
        if (isFlicking == value) return;

        isFlicking = value;
    }
}
