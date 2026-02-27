using Unity.Mathematics;
using UnityEngine;

public class BoatShaker : MonoBehaviour
{
    private const float shakeAmplitude = 5f;
    private const float shakeSpeed = 1f;

    private Vector3 shakeRotation = Vector3.zero;

    private void Update()
    {
        ProcessShake();
        ApplyShake();
    }

    private void ProcessShake()
    {
        Vector3 windDirection = WindManager.Instance.windDirection;
        shakeRotation = windDirection * math.sin(Time.time * shakeSpeed) * shakeAmplitude;
    }

    private void ApplyShake()
    {
        Quaternion rotation = Quaternion.Euler(shakeRotation);
        transform.localRotation = rotation;
    }
}
