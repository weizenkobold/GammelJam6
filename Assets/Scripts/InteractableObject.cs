using UnityEngine;

/// <summary>
/// Example implementation of an interactable object
/// </summary>
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactionText = "Press E to interact";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool oneTimeUse = false;
    
    [Header("Audio")]
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private float audioVolume = 1f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTrigger = "Interact";
    
    private bool hasBeenUsed = false;
    private AudioSource audioSource;
    
    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && interactionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public virtual void Interact(Playercontroller player)
    {
        if (!CanInteract()) return;
        
        Debug.Log($"Interacted with {gameObject.name}");
        
        // Play sound
        if (interactionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactionSound, audioVolume);
        }
        
        // Play animation
        if (animator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            animator.SetTrigger(animationTrigger);
        }
        
        // Call interaction event
        OnInteract(player);
        
        // Mark as used if one-time use
        if (oneTimeUse)
        {
            hasBeenUsed = true;
            canInteract = false;
        }
    }
    
    public virtual string GetInteractionText()
    {
        return interactionText;
    }
    
    public virtual bool CanInteract()
    {
        return canInteract && (!oneTimeUse || !hasBeenUsed);
    }
    
    /// <summary>
    /// Override this method in derived classes for custom interaction behavior
    /// </summary>
    /// <param name="player">The player controller</param>
    protected virtual void OnInteract(Playercontroller player)
    {
        // Custom interaction logic goes here
        // Example: Open door, pick up item, trigger event, etc.
    }
    
    /// <summary>
    /// Enable or disable interaction
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public void SetInteractionEnabled(bool enable)
    {
        canInteract = enable;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw interaction indicator
        Gizmos.color = CanInteract() ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}

/// <summary>
/// Example of a door that can be opened
/// </summary>
[System.Serializable]
public class Door : InteractableObject
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private string requiredKeyName = "DoorKey";
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isMoving = false;
    
    protected override void Start()
    {
        base.Start();
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }
    
    protected override void OnInteract(Playercontroller player)
    {
        if (requiresKey)
        {
            // Check if player has the required key
            // This would need to be implemented based on your inventory system
            Debug.Log($"Door requires key: {requiredKeyName}");
            return;
        }
        
        if (!isMoving)
        {
            ToggleDoor();
        }
    }
    
    public override string GetInteractionText()
    {
        if (requiresKey)
            return $"Requires {requiredKeyName}";
        
        return isOpen ? "Close Door" : "Open Door";
    }
    
    private void ToggleDoor()
    {
        isOpen = !isOpen;
        StartCoroutine(MoveDoor());
    }
    
    private System.Collections.IEnumerator MoveDoor()
    {
        isMoving = true;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed);
            yield return null;
        }
        
        transform.rotation = targetRotation;
        isMoving = false;
    }
}

/// <summary>
/// Example of a pickupable item
/// </summary>
[System.Serializable]
public class PickupItem : InteractableObject
{
    [Header("Pickup Settings")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private bool destroyOnPickup = true;
    
    protected override void OnInteract(Playercontroller player)
    {
        // Add item to inventory (would need inventory system)
        Debug.Log($"Picked up: {itemName}");
        
        // Add fear if it's a scary item
        if (itemName.ToLower().Contains("ghost") || itemName.ToLower().Contains("spirit"))
        {
            player.AddFear(10f);
        }
        
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
    
    public override string GetInteractionText()
    {
        return $"Pick up {itemName}";
    }
}