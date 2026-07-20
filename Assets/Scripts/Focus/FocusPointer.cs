using System;
using UnityEngine;

public class FocusPointer : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private GameObject arrow;
    [SerializeField] private float targetArrowHeight = 0f;
    [SerializeField] private float targetArrowScale = 0f;
    [SerializeField] private float moveArrowSpeed = 1f;
    [SerializeField] private float scaleArrowSpeed = 1f;

    [Header("Circle")]
    [SerializeField] private GameObject circle;
    [SerializeField] private float minCircleScale = 1f;
    [SerializeField] private float maxCircleScale = 1.5f;
    [SerializeField] private float scaleCircleSpeed = 1f;

    private void OnEnable()
    {
        FocusManager.Instance.RegisterPointer(this);
    }

    private void OnDisable()
    {
        FocusManager.Instance.UnregisterPointer(this);
    }

    public void Tick()
    {
        UpdateArrowPosition();
        UpdateArrowScale();
        UpdateCircle();
    }

    private void UpdateArrowPosition()
    {
        var currentPosition = arrow.transform.localPosition;
        var targetPosition = new Vector3(currentPosition.x, targetArrowHeight, currentPosition.z);

        currentPosition = Vector3.Lerp(currentPosition, targetPosition, moveArrowSpeed * Time.deltaTime);
        arrow.transform.localPosition = currentPosition;
    }

    private void UpdateArrowScale()
    {
        var currentScale = arrow.transform.localScale;
        var targetScale = new Vector3(targetArrowScale, targetArrowScale, targetArrowScale);

        currentScale = Vector3.Lerp(currentScale, targetScale, scaleArrowSpeed * Time.deltaTime);
        arrow.transform.localScale = currentScale;
    }

    private void UpdateCircle()
    {
        var alpha = Mathf.Sin(Time.time * scaleCircleSpeed) / 2 + 0.5f;
        var scale = Mathf.Lerp(minCircleScale, maxCircleScale, alpha);
        circle.transform.localScale = new Vector3(scale, scale, scale);
    }
}