using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    [SerializeField] private float pulseIntensity = 0.1f;
    [SerializeField] private float pulseSpeed = 1f;

    private Vector3 startScale;

    private void Start()
    {
        startScale = transform.localScale;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float scaleMultiplier = t * pulseIntensity;
        Vector3 scale = Vector3.one * scaleMultiplier;

        transform.localScale = startScale + scale;
    }
}