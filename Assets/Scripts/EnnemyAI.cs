using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    public enum IdleBehavior { Static, Rotation90, AvantArriere }

    [Header("Comportement Idle")]
    public IdleBehavior idleBehavior = IdleBehavior.Static;
    public float patrolDistance = 3f;
    public float patrolSpeed = 2f;
    public float rotationInterval = 2f;
    private float nextRotationTime;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private int direction = 1;
    private Quaternion targetRotation;

    [Header("Detection")]
    public float viewRadius = 8f;
    public float viewAngle = 70f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Chase")]
    public float chaseSpeed = 4f;
    public float rotationSpeed = 6f;

    [Header("Vision Origin")]
    public Transform eye;

    private Transform player;
    private CharacterController controller;
    private bool playerSpotted = false;
    private bool isStopped = false;
    private bool end = false;
    private Vector3 velocity;
    private bool isGrounded;

    // États de l’ennemi
    private enum State { Idle, Chase, Return, Waiting }
    private State currentState = State.Idle;

    private bool isTurning = false;

    // Mémoire du chemin de poursuite
    private Stack<Vector3> chasePath = new Stack<Vector3>();
    private float pathRecordInterval = 0.1f;
    private float nextRecordTime = 0f;

    // Attente avant retour
    private float waitTimer = 0f;
    private const float waitDuration = 3f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (eye == null) eye = transform;

        //startPosition = transform.position;
        //startRotation = transform.rotation;
        targetRotation = transform.rotation;
        nextRotationTime = Time.time + rotationInterval;

        GameManager.OnLevelWon += StopEnemy;
    }

    void Update()
    {
        GameObject menu = GameObject.FindGameObjectWithTag("Menu");
        isStopped = (menu != null && menu.activeInHierarchy);
        if (isStopped || end) return;

        DetectPlayer();

        switch (currentState)
        {
            case State.Chase:
                if (playerSpotted)
                    ChasePlayer();
                else
                    EnterWaitState();
                break;

            case State.Waiting:
                WaitBeforeReturn();
                break;

            case State.Return:
                ReturnToStart();
                break;

            case State.Idle:
                if (playerSpotted)
                {
                    currentState = State.Chase;
                }
                else
                {
                    Idle();
                }
                break;
        }

        HandleGravity();
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
                float dist = Vector3.Distance(eye.position, t.transform.position);

                if (!Physics.Raycast(eye.position, dirToTarget, dist, obstacleMask))
                {
                    playerSpotted = true;
                    if (currentState != State.Chase)
                        currentState = State.Chase;
                    return;
                }
            }
        }
    }

    void ChasePlayer()
    {
        if (Time.time >= nextRecordTime)
        {
            chasePath.Push(transform.position);
            nextRecordTime = Time.time + pathRecordInterval;
        }

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        controller.Move(transform.forward * chaseSpeed * Time.deltaTime);
    }

    void EnterWaitState()
    {
        currentState = State.Waiting;
        waitTimer = 0f;
    }

    void WaitBeforeReturn()
    {
        if (playerSpotted)
        {
            currentState = State.Chase;
            return;
        }

        waitTimer += Time.deltaTime;
        if (waitTimer >= waitDuration)
        {
            currentState = State.Return;
        }
    }

    void ReturnToStart()
    {
        // Si on revoit le joueur, on repart en chasse
        if (playerSpotted)
        {
            currentState = State.Chase;
            return;
        }

        if (chasePath.Count > 0)
        {
            Vector3 targetPos = chasePath.Peek();
            Vector3 moveDir = (targetPos - transform.position);
            moveDir.y = 0;

            if (moveDir.magnitude < 0.2f)
            {
                chasePath.Pop();
                return;
            }

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            controller.Move(moveDir.normalized * (chaseSpeed * 0.8f) * Time.deltaTime);
        }
        else
        {
            // Arrivé au point d’origine
            Quaternion targetRot = Quaternion.Slerp(transform.rotation, startRotation, Time.deltaTime * rotationSpeed);
            transform.rotation = targetRot;

            if (Quaternion.Angle(transform.rotation, startRotation) < 1f)
            {
                currentState = State.Idle;
            }
        }
    }

    void Idle()
    {
        switch (idleBehavior)
        {
            case IdleBehavior.Static:
                break;

            case IdleBehavior.Rotation90:
                Rotate90Behavior();
                break;

            case IdleBehavior.AvantArriere:
                PatrolBehavior();
                break;
        }
    }

    void Rotate90Behavior()
    {
        if (Time.time >= nextRotationTime)
        {
            targetRotation *= Quaternion.Euler(0, 90, 0);
            nextRotationTime = Time.time + rotationInterval;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void PatrolBehavior()
    {
        // Axe de patrouille fixe à partir de la rotation initiale
        Vector3 patrolDir = startRotation * Vector3.forward;
        Vector3 targetPos = startPosition + patrolDir * patrolDistance * direction;
        Vector3 moveDir = targetPos - transform.position;
        moveDir.y = 0f;

        // Si on est proche du point de destination
        if (!isTurning && moveDir.magnitude < 0.1f)
        {
            isTurning = true; // on entre en phase de rotation
            direction *= -1;
            targetRotation = Quaternion.LookRotation(patrolDir * direction);
            return;
        }

        // Si on est en train de tourner, faire la rotation sur place
        if (isTurning)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * 100f * Time.deltaTime);

            // Vérifier si la rotation est terminée
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation; // verrouiller proprement
                isTurning = false; // rotation terminée, reprise du déplacement
            }
            return; // ne pas avancer pendant la rotation
        }

        // Si on n’est pas en train de tourner, continuer à avancer tout droit
        controller.Move(transform.forward * patrolSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
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

    void OnDestroy()
    {
        GameManager.OnLevelWon -= StopEnemy;
    }

    private void StopEnemy()
    {
        end = true;
    }

    public void ResetStartPosition()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }


    private void HandleGravity()
    {
        // Vérifie si l'ennemi touche le sol
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            // On remet une petite valeur négative pour "coller" au sol sans rebond
            velocity.y = -2f;
        }
        else
        {
            // Sinon on applique une accélération progressive
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }

        // Applique la gravité au CharacterController
        controller.Move(velocity * Time.deltaTime);
    }


}
