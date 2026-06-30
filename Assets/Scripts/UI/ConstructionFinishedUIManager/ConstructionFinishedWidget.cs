using System;
using UnityEngine;

public class ConstructionFinishedWidget : MonoBehaviour
{
    [SerializeField] private TextLocalizer text;
    [SerializeField] private float timeToStartFading = 2f;
    [SerializeField] private float fadingSpeed = 1f;

    public ILocalizable Localizable { get; private set; }

    private float currentTimeToStartFading = 0f;
    private float currentColorAlpha = 1f;

    public static event Action<ConstructionFinishedWidget> OnWidgetDestroyed;

    public void Init(ILocalizable localizable)
    {
        Localizable = localizable;

        text.SetPlaceHolderLocalization(localizable);
    }

    public void Tick()
    {
        currentTimeToStartFading += Time.deltaTime;
        if (currentTimeToStartFading < timeToStartFading) return;

        currentColorAlpha = Mathf.Lerp(currentColorAlpha, 0f, fadingSpeed * Time.deltaTime);
        text.TextBlock.alpha = currentColorAlpha;

        TryDestroy();
    }

    private void TryDestroy()
    {
        if (currentColorAlpha > 0.01f) return;

        Destroy(gameObject);
        OnWidgetDestroyed?.Invoke(this);
    }
}