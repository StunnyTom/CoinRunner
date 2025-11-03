using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CharaControl : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.8f;
    public CharacterController controller;

    private PlayerInputActions controls;
    private Vector3 velocity;
    private bool isGrounded;

    private bool isWalking;
    private bool isSprinting;

    private Animator animator;
    [Header("Animator Params")]
    public string paramWalking = "IsWalking";
    public string paramSprinting = "IsSprinting";

    [Header("Caméra TPS")]
    public float lookSpeed = 0.2f;

    [Header("Menu de pause")]
    public GameObject menu;

    private int hashWalking = -1;
    private int hashSprinting = -1;
    private bool hasWalkingParam = false;
    private bool hasSprintingParam = false;


    private void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        // cache animator param hashes (case-insensitive search)
        if (animator != null)
        {
            var pars = animator.parameters;
            for (int i = 0; i < pars.Length; i++)
            {
                var p = pars[i];
                if (string.Equals(p.name, paramWalking, System.StringComparison.OrdinalIgnoreCase) && p.type == UnityEngine.AnimatorControllerParameterType.Bool)
                {
                    hashWalking = p.nameHash; hasWalkingParam = true;
                }
                if (string.Equals(p.name, paramSprinting, System.StringComparison.OrdinalIgnoreCase) && p.type == UnityEngine.AnimatorControllerParameterType.Bool)
                {
                    hashSprinting = p.nameHash; hasSprintingParam = true;
                }
            }
        }
    }

    private void Awake()
    {
        controls = new PlayerInputActions();
    }

    private void OnEnable()
    {
        controls.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        controls.PlayerControls.Disable();
    }

    private void Update()
    {
        CheckEnemyCollision();
        HandleMovement();
        HandleGravity();
        HandleLook();
        HandleMenuInput();
    }

    private void HandleLook()
    {
        if (menu != null && menu.activeSelf) return;

        Vector2 lookInput = controls.PlayerControls.Look.ReadValue<Vector2>();

        // Rotation horizontale du joueur
        transform.Rotate(Vector3.up * lookInput.x * lookSpeed);
    }


    private void HandleMovement()
    {
        if (menu != null && menu.activeSelf) return;

        // Récupération du mouvement depuis l'Event System / Input System
        Vector2 moveInput = controls.PlayerControls.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Sprint (via Left Shift)
        bool sprintPressed = false;
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb != null) sprintPressed = kb.leftShiftKey.isPressed;
        #else
            sprintPressed = Input.GetKey(KeyCode.LeftShift);
        #endif
            isSprinting = sprintPressed;

        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        // Déplacement du CharacterController
        controller.Move(move * speed * Time.deltaTime);

        // Gestion de l'animation
        bool walking = moveInput.sqrMagnitude > 0.01f;
        if (animator != null)
        {
            if (hasWalkingParam) animator.SetBool(hashWalking, walking);
            if (hasSprintingParam) animator.SetBool(hashSprinting, isSprinting);
        }
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

    private float lastMenuToggleTime = 0f;
    private float menuToggleCooldown = 0.3f;

    private void HandleMenuInput()
    {
        bool pause = false;
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb != null) pause = kb.mKey.isPressed;
        #else
            pause = Input.GetKey(KeyCode.M);
        #endif
        
        if (pause && Time.time >= lastMenuToggleTime + menuToggleCooldown)
        {
            if (menu != null)
            {
                menu.SetActive(!menu.activeSelf);
                lastMenuToggleTime = Time.time;
            }
            else
            {
                Debug.LogWarning("Menu introuvable dans la scène.");
            }
        }
    }


    private void CheckEnemyCollision()
    {
        if (GameManager.Instance.levelWon)
        {
            Debug.Log("Level already won, skipping enemy collision check.");
            return;
        }

        // On récupère tous les colliders sur la couche Enemy dans un rayon de 0.5 à 1 unité
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.8f, LayerMask.GetMask("Enemy"));
        //Debug.Log($"Enemy collision check: found {hits.Length} enemies nearby.");
        if (hits.Length > 0)
        {
            // On déclenche la défaite
            GameManager.Instance.ShowLoseScreen();
            
            // Bloquer le joueur pour éviter de continuer
            controller.enabled = false;
        }
    }
}
