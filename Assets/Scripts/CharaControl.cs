using UnityEngine;
using UnityEngine.InputSystem;

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

    private int coinCounter = 0;

    int hashWalking = -1;
    int hashSprinting = -1;
    bool hasWalkingParam = false;
    bool hasSprintingParam = false;

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
            if (!hasWalkingParam) Debug.LogWarning($"Animator on {name} has no boolean param matching '{paramWalking}' (case-insensitive).\nAnimator params: {string.Join(", ", System.Array.ConvertAll(pars, x=>x.name))}");
            if (!hasSprintingParam) Debug.LogWarning($"Animator on {name} has no boolean param matching '{paramSprinting}' (case-insensitive).\nAnimator params: {string.Join(", ", System.Array.ConvertAll(pars, x=>x.name))}");
        }
    }

    private void Awake()
    {
        // Instancie la classe générée par ton .inputactions
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
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = controls.PlayerControls.Move.ReadValue<Vector2>();
        float droitegauche = moveInput.x;
        float avantarr = moveInput.y;

        // Sprint detection (Left Shift) - support new Input System and legacy
        bool sprintPressed = false;
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb != null) sprintPressed = kb.leftShiftKey.isPressed;
        #else
            sprintPressed = Input.GetKey(KeyCode.LeftShift);
        #endif
        isSprinting = sprintPressed;

        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        // Déplacement horizontal avant/arrière
        Vector3 forwardMove = transform.forward * avantarr;
        controller.Move(forwardMove * speed * Time.deltaTime);

        // Rotation seulement sur l'axe Y selon X
        if (Mathf.Abs(droitegauche) > 0.001f)
        {
            const float rotationSpeed = 320f;

            // On calcule la rotation cible relative à l'orientation actuelle
            float targetY = transform.eulerAngles.y + droitegauche * 90f;
            Quaternion targetRot = Quaternion.Euler(0f, targetY, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Animation
        bool walking = moveInput.sqrMagnitude > 0.0001f;
        if (animator != null)
        {
            if (hasWalkingParam) animator.SetBool(hashWalking, walking);
            if (hasSprintingParam) animator.SetBool(hashSprinting, isSprinting);
        }
    }

    private void HandleGravity()
    {
        // Vérifier si le personnage touche le sol (approximatif)
        isGrounded = controller.transform.position.y <= 0.2f;

        // Appliquer une petite vitesse négative au sol pour rester collé (utile à l'arrêt)
        if (isGrounded)
            velocity.y = -2f;

        // Appliquer la gravité
        velocity.y += Physics.gravity.y * Time.deltaTime;

        // Déplacement vertical
        controller.Move(velocity * Time.deltaTime);
    }
}
