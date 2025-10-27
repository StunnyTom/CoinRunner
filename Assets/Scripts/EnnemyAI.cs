using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float viewRadius = 8f;
    public float viewAngle = 70f;
    public LayerMask targetMask;   // Player layer
    public LayerMask obstacleMask; // Wall layer

    [Header("Chase")]
    public float chaseSpeed = 4f;
    public float rotationSpeed = 6f;

    [Header("Vision Origin")]
    public Transform eye;

    private Transform player;
    private CharacterController controller;
    private bool playerSpotted = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (eye == null) eye = transform;
    }

    void Update()
    {
        // Check vision
        DetectPlayer();

        // Chase if seen
        if (playerSpotted)
            ChasePlayer();
        else
            Idle();
    }

    void DetectPlayer()
    {
        playerSpotted = false;

        Collider[] targets = Physics.OverlapSphere(eye.position, viewRadius, targetMask);

        foreach (var t in targets)
        {
            Vector3 dirToTarget = (t.transform.position - eye.position).normalized;

            if (Vector3.Angle(eye.forward, dirToTarget) < viewAngle / 2)
            {
                float dist = Vector3.Distance(eye.position, t.transform.position); // <- ici

                // Wall blocking test
                if (!Physics.Raycast(eye.position, dirToTarget, dist, obstacleMask))
                {
                    playerSpotted = true;
                    return;
                }
            }
        }
    }


    void ChasePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        // Rotate smoothly
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move forward
        controller.Move(transform.forward * chaseSpeed * Time.deltaTime);
    }

    void Idle()
    {
        // Optional: Patrol or stay idle
        // You can later add a patrol path here
    }

    private void OnDrawGizmosSelected()
    {
        // Debug view of cone
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 right = DirFromAngle(viewAngle / 2);
        Vector3 left = DirFromAngle(-viewAngle / 2);

        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
    }

    Vector3 DirFromAngle(float angle)
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
