using UnityEngine;

/// <summary>
/// Setup script to create the necessary input actions for the player controller
/// This script provides the input mapping configuration needed for the player controller
/// </summary>
[CreateAssetMenu(fileName = "PlayerInputActions", menuName = "Horror Game/Player Input Actions")]
public class PlayerInputSetup : ScriptableObject
{
    [System.Serializable]
    public class InputActionSetup
    {
        public string actionName;
        public string binding;
        public string description;
    }
    
    [Header("Required Input Actions")]
    [SerializeField] private InputActionSetup[] requiredActions = new InputActionSetup[]
    {
        new InputActionSetup { actionName = "Move", binding = "WASD", description = "Movement input (Vector2)" },
        new InputActionSetup { actionName = "Look", binding = "Mouse Delta", description = "Mouse look input (Vector2)" },
        new InputActionSetup { actionName = "Jump", binding = "Space", description = "Jump action" },
        new InputActionSetup { actionName = "Run", binding = "Left Shift", description = "Run/Sprint action" },
        new InputActionSetup { actionName = "Crouch", binding = "Left Ctrl", description = "Crouch action" },
        new InputActionSetup { actionName = "Flashlight", binding = "F", description = "Toggle flashlight" },
        new InputActionSetup { actionName = "Interact", binding = "E", description = "Interact with objects" }
    };
    
    [Header("Input Action Asset")]
    [SerializeField] private UnityEngine.InputSystem.InputActionAsset inputActionAsset;
    
    public InputActionSetup[] GetRequiredActions()
    {
        return requiredActions;
    }
    
    public UnityEngine.InputSystem.InputActionAsset GetInputActionAsset()
    {
        return inputActionAsset;
    }
    
    [ContextMenu("Print Input Setup Instructions")]
    public void PrintSetupInstructions()
    {
        Debug.Log("=== PLAYER CONTROLLER INPUT SETUP INSTRUCTIONS ===");
        Debug.Log("1. Create a new Input Action Asset in your project");
        Debug.Log("2. Add the following action map and actions:");
        Debug.Log("");
        Debug.Log("Action Map: Player");
        
        foreach (var action in requiredActions)
        {
            Debug.Log($"  - {action.actionName}: {action.binding} ({action.description})");
        }
        
        Debug.Log("");
        Debug.Log("3. Assign the Input Action Asset to the PlayerInput component");
        Debug.Log("4. Set the Default Action Map to 'Player'");
        Debug.Log("5. Make sure Auto-Switch is enabled if using multiple input schemes");
    }
}

/// <summary>
/// Helper component to automatically setup player input if no PlayerInput component exists
/// </summary>
[System.Serializable]
public class InputFallback : MonoBehaviour
{
    [Header("Fallback Input Settings")]
    [SerializeField] private bool useFallbackInput = true;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode flashlightKey = KeyCode.F;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    private Playercontroller playerController;
    
    void Start()
    {
        playerController = GetComponent<Playercontroller>();
        
        if (playerController != null && GetComponent<UnityEngine.InputSystem.PlayerInput>() == null)
        {
            Debug.LogWarning("No PlayerInput component found. Using fallback input system.");
            Debug.LogWarning("For better input handling, please set up Input System with the required actions.");
        }
    }
    
    public bool UseFallbackInput()
    {
        return useFallbackInput && GetComponent<UnityEngine.InputSystem.PlayerInput>() == null;
    }
    
    public KeyCode GetJumpKey() => jumpKey;
    public KeyCode GetRunKey() => runKey;
    public KeyCode GetCrouchKey() => crouchKey;
    public KeyCode GetFlashlightKey() => flashlightKey;
    public KeyCode GetInteractKey() => interactKey;
}