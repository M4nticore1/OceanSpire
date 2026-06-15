using Unity.Mathematics;
using UnityEngine;

public class BoatShaker : MonoBehaviour
{
    [SerializeField] private float shakeAmplitude = 5f;
    [SerializeField] private float shakeSpeed = 1f;

    private Vector3 shakeRotation = Vector3.zero;

    public void Tick()
    {
        ProcessShake();
        ApplyShake();
    }

    private void ProcessShake()
    {
        Vector3 windDirection = WindManager.Instance.WindDirection;
        shakeRotation = windDirection * math.sin(Time.time * shakeSpeed) * shakeAmplitude;
    }

    private void ApplyShake()
    {
        Quaternion rotation = Quaternion.Euler(shakeRotation);
        transform.localRotation = rotation;
    }
}
