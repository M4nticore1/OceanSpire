using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    [SerializeField] private float pulseIntensity = 0.1f;
    [SerializeField] private float pulseSpeed = 1f;

    private Vector3 startScale;
    private PulseEffectsManager pulseEffectsManager => PulseEffectsManager.Instance;

    private void OnDestroy()
    {
        pulseEffectsManager.UnregisterEffect(this);
    }

    private void Start()
    {
        pulseEffectsManager.RegisterEffect(this);
        startScale = transform.localScale;
    }

    public void Tick()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float scaleMultiplier = t * pulseIntensity;
        Vector3 scale = Vector3.one * scaleMultiplier;

        transform.localScale = startScale + scale;
    }
}