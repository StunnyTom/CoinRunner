using UnityEngine;

public class Cam : MonoBehaviour
{
    public Transform target;

    [Header("Smoothing")]
    public float rotationSmooth = 6f;
    public float positionSmooth = 8f;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 2f, -4f);

    [Header("Collisions caméra")]
    public LayerMask collisionMask; // Walls layer
    public float cameraRadius = 0.3f; // small buffer

    void LateUpdate()
    {
        if (target == null) return;

        // Desired rotation 
        Quaternion desiredRot = Quaternion.Euler(20f, target.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);

        // Desired camera position
        Vector3 desiredPos = target.position + desiredRot * offset;

        // CHECK COLLISION VIA RAYCAST
        Vector3 direction = desiredPos - target.position;
        float distance = direction.magnitude;
        direction.Normalize();

        // If raycast hits something, reposition camera at hit point
        if (Physics.SphereCast(target.position, cameraRadius, direction, out RaycastHit hit, distance, collisionMask))
        {
            desiredPos = hit.point + hit.normal * cameraRadius;
        }

        // Smooth camera follow
        transform.position = Vector3.Lerp(transform.position, desiredPos, positionSmooth * Time.deltaTime);
    }
}
