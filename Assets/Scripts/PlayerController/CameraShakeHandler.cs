using Unity.Mathematics;
using UnityEngine;

public class CameraShakeHandler : MonoBehaviour
{
    private const float cameraShakeAmplitude = 1f;
    private const float cameraShakeSpeed = 0.5f;
    private Vector3 currentCameraShakeForce = Vector3.zero;
    private Vector3 currentCameraShakeRotation = Vector3.zero;

    private void Update()
    {
        ResetShake();
        ProcessShake();
        ApplyShake();
    }

    private void ProcessShake()
    {
        currentCameraShakeForce = new Vector3(math.sin(Time.time * cameraShakeSpeed) * cameraShakeAmplitude, math.cos(Time.time * cameraShakeSpeed / 2) * cameraShakeAmplitude, 0);
        currentCameraShakeRotation = math.lerp(currentCameraShakeRotation, currentCameraShakeForce, cameraShakeSpeed * Time.deltaTime);
    }

    private void ApplyShake()
    {
        Vector3 rotation = transform.rotation.eulerAngles + currentCameraShakeRotation;
        transform.rotation = Quaternion.Euler(rotation);
    }

    private void ResetShake()
    {
        Vector3 rotation = transform.rotation.eulerAngles - currentCameraShakeRotation;
        transform.rotation = Quaternion.Euler(rotation);
    }
}
