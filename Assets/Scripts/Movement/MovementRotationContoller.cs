using UnityEngine;

public class MovementRotationContoller : MonoBehaviour
{
    [SerializeField] private Movement movement;
    [SerializeField] private float rotationSpeed = 1f;

    //private void Update()
    //{
    //    if (!ShouldRotate()) return;

    //    transform.rotation = Quaternion.Lerp(transform.rotation, movement.TargetRotation, rotationSpeed * Time.deltaTime);
    //}

    //private bool ShouldRotate()
    //{
    //    if (movement.IsMoving) return false;
    //    if (!movement.UseTargetRotation) return false;

    //    return true;
    //}
}