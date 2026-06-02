using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam = null;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (!cam) return;

        transform.LookAt(cam.transform.position, cam.transform.up);

        if (!transform.parent) return;

        //Vector3 rotation = billboardWorld.eulerAngles;
        //Vector3 parentRotation = transform.parent.rotation.eulerAngles;

        //transform.rotation = billboardWorld;
        transform.rotation *= transform.parent.rotation;
    }
}
