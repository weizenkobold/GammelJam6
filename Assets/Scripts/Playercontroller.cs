using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class Playercontroller : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer = 1;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upDownRange = 80f;
    [SerializeField] private bool invertY = false;
    
    [Header("Crouching")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 8f;
    
    [Header("Head Bob")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobAmplitudeRunning = 0.1f;
    
    [Header("Stamina System")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float minStaminaToRun = 10f;
    
    [Header("Flashlight")]
    [SerializeField] private Light flashlight;
    [SerializeField] private AudioClip flashlightClickSound;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] woodFootsteps;
    [SerializeField] private AudioClip[] stoneFootsteps;
    [SerializeField] private float footstepVolume = 0.5f;
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("Fear System")]
    [SerializeField] private float fearLevel = 0f;
    [SerializeField] private float maxFear = 100f;
    [SerializeField] private float fearDecayRate = 2f;
    [SerializeField] private AnimationCurve fearShakeCurve;
    
    // Private variables
    private CharacterController characterController;
    private AudioSource audioSource;
    private Vector3 moveDirection;
    private Vector2 currentInput;
    private float verticalRotation;
    private bool isCrouching;
    private bool isRunning;
    private bool isGrounded;
    private float currentStamina;
    private bool canRun = true;
    
    // Head bob
    private Vector3 originalCameraPos;
    private float bobTimer;
    
    // Interaction
    private GameObject currentInteractable;
    private PlayerUI playerUI;
    
    // Input System
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction flashlightAction;
    private InputAction interactAction;
    
    // Fear effects
    private float fearShakeTimer;
    private Vector3 fearShakeOffset;
    
    void Awake()
    {
        // Get components
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        
        // Get camera if not assigned
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        // Initialize values
        currentStamina = maxStamina;
        originalCameraPos = playerCamera.transform.localPosition;
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Setup input actions
        SetupInputActions();
    }
    
    void Start()
    {
        // Initialize flashlight state
        if (flashlight != null)
            flashlight.enabled = false;
            
        // Find UI component
        playerUI = FindFirstObjectByType<PlayerUI>();
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleCamera();
        HandleCrouching();
        HandleStamina();
        HandleHeadBob();
        HandleInteraction();
        HandleFearEffects();
        CheckGrounded();
    }
    
    void SetupInputActions()
    {
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            jumpAction = playerInput.actions["Jump"];
            runAction = playerInput.actions["Run"];
            crouchAction = playerInput.actions["Crouch"];
            flashlightAction = playerInput.actions["Flashlight"];
            interactAction = playerInput.actions["Interact"];
        }
    }
    
    void HandleInput()
    {
        // Movement input
        if (moveAction != null)
            currentInput = moveAction.ReadValue<Vector2>();
        else
            currentInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        // Running
        if (runAction != null)
            isRunning = runAction.IsPressed() && canRun && currentStamina > minStaminaToRun;
        else
            isRunning = Input.GetKey(KeyCode.LeftShift) && canRun && currentStamina > minStaminaToRun;
        
        // Jumping
        bool jumpPressed = false;
        if (jumpAction != null)
            jumpPressed = jumpAction.WasPressedThisFrame();
        else
            jumpPressed = Input.GetKeyDown(KeyCode.Space);
        
        if (jumpPressed && isGrounded && !isCrouching)
            Jump();
        
        // Crouching
        if (crouchAction != null)
        {
            if (crouchAction.WasPressedThisFrame())
                ToggleCrouch();
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
            ToggleCrouch();
        
        // Flashlight
        if (flashlightAction != null)
        {
            if (flashlightAction.WasPressedThisFrame())
                ToggleFlashlight();
        }
        else if (Input.GetKeyDown(KeyCode.F))
            ToggleFlashlight();
        
        // Interaction
        if (interactAction != null)
        {
            if (interactAction.WasPressedThisFrame())
                TryInteract();
        }
        else if (Input.GetKeyDown(interactKey))
            TryInteract();
    }
    
    void HandleMovement()
    {
        if (isGrounded && moveDirection.y < 0)
            moveDirection.y = -2f;
        
        // Get movement speed
        float currentSpeed = GetCurrentSpeed();
        
        // Calculate movement direction
        Vector3 move = transform.right * currentInput.x + transform.forward * currentInput.y;
        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;
        
        // Apply gravity
        moveDirection.y += gravity * Time.deltaTime;
        
        // Move the character
        characterController.Move(moveDirection * Time.deltaTime);
        
        // Handle footsteps
        if (isGrounded && move.magnitude > 0.1f)
            HandleFootsteps();
    }
    
    float GetCurrentSpeed()
    {
        if (isCrouching)
            return crouchSpeed;
        else if (isRunning)
            return runSpeed;
        else
            return walkSpeed;
    }
    
    void HandleCamera()
    {
        // Mouse look input
        Vector2 mouseDelta;
        if (lookAction != null)
            mouseDelta = lookAction.ReadValue<Vector2>();
        else
            mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        // Apply sensitivity
        mouseDelta *= mouseSensitivity;
        
        // Horizontal rotation (Y-axis)
        transform.Rotate(Vector3.up * mouseDelta.x);
        
        // Vertical rotation (X-axis)
        if (invertY)
            verticalRotation += mouseDelta.y;
        else
            verticalRotation -= mouseDelta.y;
        
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    
    void HandleCrouching()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, 
            crouchTransitionSpeed * Time.deltaTime);
        
        // Adjust camera position
        float heightDifference = standingHeight - characterController.height;
        Vector3 newPos = originalCameraPos;
        newPos.y -= heightDifference * 0.5f;
        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, 
            newPos, crouchTransitionSpeed * Time.deltaTime);
    }
    
    void HandleStamina()
    {
        if (isRunning && currentInput.magnitude > 0.1f)
        {
            // Drain stamina when running
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            
            if (currentStamina <= 0)
                canRun = false;
        }
        else
        {
            // Regenerate stamina
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
            
            if (currentStamina >= minStaminaToRun)
                canRun = true;
        }
    }
    
    void HandleHeadBob()
    {
        if (!enableHeadBob || !isGrounded)
            return;
        
        if (currentInput.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float amplitude = isRunning ? bobAmplitudeRunning : bobAmplitude;
            
            Vector3 bobOffset = new Vector3(
                Mathf.Sin(bobTimer) * amplitude * 0.5f,
                Mathf.Sin(bobTimer * 2) * amplitude,
                0
            );
            
            playerCamera.transform.localPosition = originalCameraPos + bobOffset + fearShakeOffset;
        }
        else
        {
            bobTimer = 0;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                originalCameraPos + fearShakeOffset,
                Time.deltaTime * 4f
            );
        }
    }
    
    void HandleInteraction()
    {
        // Raycast for interactables
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            GameObject hitObject = hit.collider.gameObject;
            IInteractable interactable = hitObject.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                if (currentInteractable != hitObject)
                {
                    currentInteractable = hitObject;
                    // Show interaction UI
                    if (playerUI != null)
                        playerUI.ShowInteractionPrompt(interactable.GetInteractionText());
                }
            }
            else
            {
                if (currentInteractable != null)
                {
                    currentInteractable = null;
                    // Hide interaction UI
                    if (playerUI != null)
                        playerUI.HideInteractionPrompt();
                }
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;
                // Hide interaction UI
                if (playerUI != null)
                    playerUI.HideInteractionPrompt();
            }
        }
    }
    
    void HandleFearEffects()
    {
        // Decay fear over time
        fearLevel -= fearDecayRate * Time.deltaTime;
        fearLevel = Mathf.Max(0, fearLevel);
        
        // Apply fear shake
        if (fearLevel > 0)
        {
            fearShakeTimer += Time.deltaTime * (fearLevel / maxFear) * 10f;
            float shakeIntensity = fearShakeCurve.Evaluate(fearLevel / maxFear) * 0.02f;
            
            fearShakeOffset = new Vector3(
                Mathf.Sin(fearShakeTimer * 20f) * shakeIntensity,
                Mathf.Sin(fearShakeTimer * 25f) * shakeIntensity,
                0
            );
        }
        else
        {
            fearShakeOffset = Vector3.zero;
        }
    }
    
    void HandleFootsteps()
    {
        // Simple footstep timer
        float stepInterval = isRunning ? 0.3f : 0.5f;
        if (isCrouching) stepInterval = 0.8f;
        
        if (Time.time - lastFootstepTime > stepInterval)
        {
            PlayFootstepSound();
            lastFootstepTime = Time.time;
        }
    }
    
    private float lastFootstepTime;
    
    void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0) return;
        
        AudioClip clipToPlay = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(clipToPlay, footstepVolume);
    }
    
    void CheckGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, 
            transform.position.y - characterController.height/2 + groundCheckRadius, 
            transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer);
    }
    
    void Jump()
    {
        moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    
    void ToggleCrouch()
    {
        isCrouching = !isCrouching;
    }
    
    void ToggleFlashlight()
    {
        if (flashlight != null)
        {
            flashlight.enabled = !flashlight.enabled;
            
            if (flashlightClickSound != null)
                audioSource.PlayOneShot(flashlightClickSound);
        }
    }
    
    void TryInteract()
    {
        if (currentInteractable != null)
        {
            IInteractable interactable = currentInteractable.GetComponent<IInteractable>();
            interactable?.Interact(this);
        }
    }
    
    // Public methods for other scripts
    public void AddFear(float amount)
    {
        fearLevel += amount;
        fearLevel = Mathf.Min(maxFear, fearLevel);
    }
    
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    
    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }
    
    public float GetFearLevel()
    {
        return fearLevel / maxFear;
    }
    
    public bool IsRunning()
    {
        return isRunning;
    }
    
    public bool IsCrouching()
    {
        return isCrouching;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw ground check
        Vector3 spherePosition = new Vector3(transform.position.x, 
            transform.position.y - (characterController ? characterController.height/2 : 1f) + groundCheckRadius, 
            transform.position.z);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
        
        // Draw interaction range
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionRange);
    }
}
