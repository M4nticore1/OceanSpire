using System;
using UnityEngine;

public class PlayerClickShaker : MonoBehaviour, IClickable
{
    [SerializeField] private Transform shakeTransform;
    [SerializeField] private float shakeAmplitude = 5f;
    [SerializeField] private float shakeSpeed = 1f;
    [SerializeField] private float shakeStartSpeed = 5f;
    [SerializeField] private float shakeStopSpeed = 1f;

    private bool isShaking = false;
    private float shakeIntensity = 0;

    public bool IsClickable { get; private set; }
    public event Action OnClicked;

    private void OnEnable()
    {
        PlayerClickShakeManager.Instance.RegisterShaker(this);
    }

    private void OnDisable()
    {
        PlayerClickShakeManager.Instance.UnregisterShaker(this);
    }

    public void Tick()
    {
        if (isShaking) {
            shakeIntensity += shakeStartSpeed * Time.deltaTime;

            if (shakeIntensity >= 1f) {
                shakeIntensity = 1f;
                SetShaking(false);
            }
        }

        float rotationX = Mathf.Sin(Time.time * shakeSpeed);
        float rotationY = Mathf.Sin(Time.time * shakeSpeed);
        float rotationZ = Mathf.Sin(Time.time * shakeSpeed);

        var rotation = new Vector3(rotationX, rotationY, rotationZ) * shakeAmplitude * shakeIntensity;
        shakeTransform.localRotation = Quaternion.Euler(rotation);

        if (!isShaking) {
            shakeIntensity -= shakeStopSpeed * Time.deltaTime;

            if (shakeIntensity <= 0) {
                shakeIntensity = 0f;
            }
        }
    }

    public void Click()
    {
        SetShaking(true);
        SetShakeIntensity(0f);
        OnClicked?.Invoke();
    }

    public void SetClickable(bool value)
    {
        IsClickable = value;
    }

    public bool ShouldClick()
    {
        return true;
    }

    private void SetShaking(bool value)
    {
        isShaking = value;
    }

    private void SetShakeIntensity(float value)
    {
        shakeIntensity = value;
    }
}