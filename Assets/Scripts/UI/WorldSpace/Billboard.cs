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
        if (cam)
            transform.LookAt(transform.position - cam.transform.forward, cam.transform.up);
    }
}
