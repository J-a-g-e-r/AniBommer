using UnityEngine;

public class HealthBarBillboard : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // Luôn quay mặt về camera
        transform.forward = cam.transform.forward;
    }
}