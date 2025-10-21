using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class Playercontroller : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upDownRange = 80f;
    
    [Header("Head Bob")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobAmplitudeRunning = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip flashlightClickSound;
    [SerializeField] private Light flashlight;
    
    // Private variables
    private CharacterController characterController;
    private AudioSource audioSource;
    private Vector3 moveDirection;
    private float verticalRotation;
    private bool isGrounded;
    private float lastFootstepTime;
    
    // Head bob
    private Vector3 originalCameraPos;
    private float bobTimer;
    
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        // Initialize head bob
        originalCameraPos = playerCamera.transform.localPosition;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMovement();
        HandleCamera();
        HandleFlashlight();
        HandleHeadBob();
        CheckGrounded();
    }
    
    void HandleMovement()
    {
        if (isGrounded && moveDirection.y < 0)
            moveDirection.y = -2f;
        
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Calculate movement
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;
        
        // Apply gravity
        moveDirection.y += gravity * Time.deltaTime;
        
        // Move
        characterController.Move(moveDirection * Time.deltaTime);
        
        // Handle footsteps
        if (isGrounded && move.magnitude > 0.1f)
            HandleFootsteps(isRunning);
    }
    
    void HandleCamera()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Horizontal rotation
        transform.Rotate(Vector3.up * mouseX);
        
        // Vertical rotation
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    
    void HandleFlashlight()
    {
        if (Input.GetKeyDown(KeyCode.F) && flashlight != null)
        {
            flashlight.enabled = !flashlight.enabled;
            
            if (flashlightClickSound != null)
                audioSource.PlayOneShot(flashlightClickSound);
        }
    }
    
    void HandleFootsteps(bool isRunning)
    {
        float stepInterval = isRunning ? 0.3f : 0.5f;
        
        if (Time.time - lastFootstepTime > stepInterval)
        {
            PlayFootstepSound();
            lastFootstepTime = Time.time;
        }
    }
    
    void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0) return;
        
        AudioClip clipToPlay = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(clipToPlay, 0.5f);
    }
    
    void HandleHeadBob()
    {
        if (!enableHeadBob || !isGrounded)
            return;
        
        // Get current movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 currentInput = new Vector3(horizontal, 0, vertical);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        if (currentInput.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float amplitude = isRunning ? bobAmplitudeRunning : bobAmplitude;
            
            Vector3 bobOffset = new Vector3(
                Mathf.Sin(bobTimer) * amplitude * 0.5f,
                Mathf.Sin(bobTimer * 2) * amplitude,
                0
            );
            
            playerCamera.transform.localPosition = originalCameraPos + bobOffset;
        }
        else
        {
            bobTimer = 0;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                originalCameraPos,
                Time.deltaTime * 4f
            );
        }
    }
    
    void CheckGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, 
            transform.position.y - characterController.height/2 + 0.3f, 
            transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, 0.3f);
    }
}