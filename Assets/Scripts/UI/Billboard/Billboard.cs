using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private bool useParentRotation = false;

    private Camera cam;
    private BillboardsManager billboardsManager => BillboardsManager.Instance;

    private void OnEnable()
    {
        if (billboardsManager != null) {
            BillboardsManager.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        if (billboardsManager != null) {
            BillboardsManager.Instance.Unregister(this);
        }
    }

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Tick()
    {
        if (cam == null) return;

        transform.LookAt(cam.transform.position, cam.transform.up);
        transform.Rotate(0f, 180f, 0f);

        if (useParentRotation) {
            if (transform.parent == null) return;

            transform.rotation *= transform.parent.rotation;
        }
    }
}