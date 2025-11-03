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

        // Rotation désirée de la caméra
        Quaternion desiredRot = Quaternion.Euler(20f, target.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);

        // Position désirée de la caméra 
        Vector3 desiredPos = target.position + desiredRot * offset;

        // --- COLLISION CHECK ---
        // On élève le point de départ du raycast pour éviter les petits rebords / le sol
        Vector3 rayOrigin = target.position + Vector3.up * 0.5f;  
        Vector3 direction = desiredPos - rayOrigin;
        float distance = direction.magnitude;
        direction.Normalize();

        // Si on touche un obstacle, on recule légèrement la caméra
        if (Physics.SphereCast(rayOrigin, cameraRadius, direction, out RaycastHit hit, distance, collisionMask))
        {
            desiredPos = hit.point + hit.normal * cameraRadius;
        }

        // --- Smoothing ---
        transform.position = Vector3.Lerp(transform.position, desiredPos, positionSmooth * Time.deltaTime);
    }

}
