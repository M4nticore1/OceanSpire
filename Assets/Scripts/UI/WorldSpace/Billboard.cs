using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private bool useParentRotation = false;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (!cam) return;

        transform.LookAt(cam.transform.position, cam.transform.up);

        if (useParentRotation) {
            if (!transform.parent) return;

            transform.rotation *= transform.parent.rotation;
        }
    }
}