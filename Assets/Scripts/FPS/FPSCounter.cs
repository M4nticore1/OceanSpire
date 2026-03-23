using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText = null;
    private int frames = 0;
    private double lastFPSCounterTime = 0d;
    private float elapsedTime = 0.5f;
    private bool isCounting = true;

    private void OnEnable()
    {
        FPSCounterSystem.onCountingEnabledChanged += OnCountingEnabledChanged;
    }

    private void OnDisable()
    {
        FPSCounterSystem.onCountingEnabledChanged -= OnCountingEnabledChanged;
    }

    private void Update()
    {
        if (!isCounting) return;

        frames++;
        double time = Time.timeAsDouble;

        if (time >= lastFPSCounterTime + elapsedTime) {
            double delta = time - lastFPSCounterTime;
            float fps = frames / (float)delta;
            fpsText.text = $"FPS: {(int)fps}";
            frames = 0;
            lastFPSCounterTime = time;
        }
    }

    private void OnCountingEnabledChanged(bool value)
    {
        isCounting = value;
        fpsText.gameObject.SetActive(isCounting);
    }
}