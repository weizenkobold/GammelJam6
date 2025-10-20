using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple inventory system for horror game items
/// </summary>
[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int quantity;
    public bool isStackable;
    public float weight;
    
    public InventoryItem(string name, string desc, Sprite itemIcon, int qty = 1, bool stackable = true, float itemWeight = 1f)
    {
        itemName = name;
        description = desc;
        icon = itemIcon;
        quantity = qty;
        isStackable = stackable;
        weight = itemWeight;
    }
}

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 10;
    [SerializeField] private float maxWeight = 50f;
    [SerializeField] private bool useWeightSystem = true;
    
    [Header("Special Items")]
    [SerializeField] private string[] importantItems = { "Key", "Evidence", "EMF Reader", "Spirit Box" };
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioClip errorSound;
    
    private List<InventoryItem> inventory = new List<InventoryItem>();
    private AudioSource audioSource;
    private PlayerUI playerUI;
    
    // Events
    public System.Action<InventoryItem> OnItemAdded;
    public System.Action<InventoryItem> OnItemRemoved;
    public System.Action OnInventoryChanged;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        playerUI = FindFirstObjectByType<PlayerUI>();
    }
    
    public bool AddItem(string itemName, string description = "", Sprite icon = null, int quantity = 1, bool stackable = true, float weight = 1f)
    {
        // Check if we can add the item
        if (!CanAddItem(itemName, quantity, weight))
        {
            PlayErrorSound();
            if (playerUI != null)
                playerUI.ShowMessage($"Cannot pick up {itemName} - inventory full!", 2f);
            return false;
        }
        
        // Try to stack with existing item
        if (stackable)
        {
            var existingItem = inventory.FirstOrDefault(item => item.itemName == itemName && item.isStackable);
            if (existingItem != null)
            {
                existingItem.quantity += quantity;
                PlayPickupSound();
                OnItemAdded?.Invoke(existingItem);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        
        // Add as new item
        var newItem = new InventoryItem(itemName, description, icon, quantity, stackable, weight);
        inventory.Add(newItem);
        
        PlayPickupSound();
        OnItemAdded?.Invoke(newItem);
        OnInventoryChanged?.Invoke();
        
        // Show pickup message
        if (playerUI != null)
            playerUI.ShowMessage($"Picked up: {itemName}", 1.5f);
        
        return true;
    }
    
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        var item = inventory.FirstOrDefault(i => i.itemName == itemName);
        if (item == null) return false;
        
        if (item.quantity <= quantity)
        {
            inventory.Remove(item);
            OnItemRemoved?.Invoke(item);
        }
        else
        {
            item.quantity -= quantity;
        }
        
        PlayDropSound();
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    public bool HasItem(string itemName, int requiredQuantity = 1)
    {
        var item = inventory.FirstOrDefault(i => i.itemName == itemName);
        return item != null && item.quantity >= requiredQuantity;
    }
    
    public InventoryItem GetItem(string itemName)
    {
        return inventory.FirstOrDefault(i => i.itemName == itemName);
    }
    
    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(inventory);
    }
    
    public bool CanAddItem(string itemName, int quantity, float weight)
    {
        // Check slot limit
        if (!IsStackableWithExisting(itemName) && inventory.Count >= maxSlots)
            return false;
        
        // Check weight limit
        if (useWeightSystem && GetCurrentWeight() + (weight * quantity) > maxWeight)
            return false;
        
        return true;
    }
    
    private bool IsStackableWithExisting(string itemName)
    {
        return inventory.Any(item => item.itemName == itemName && item.isStackable);
    }
    
    public float GetCurrentWeight()
    {
        return inventory.Sum(item => item.weight * item.quantity);
    }
    
    public float GetWeightPercentage()
    {
        return useWeightSystem ? GetCurrentWeight() / maxWeight : 0f;
    }
    
    public int GetUsedSlots()
    {
        return inventory.Count;
    }
    
    public int GetAvailableSlots()
    {
        return maxSlots - GetUsedSlots();
    }
    
    public bool HasImportantItem()
    {
        return inventory.Any(item => importantItems.Contains(item.itemName));
    }
    
    public List<InventoryItem> GetImportantItems()
    {
        return inventory.Where(item => importantItems.Contains(item.itemName)).ToList();
    }
    
    public void DropItem(string itemName, int quantity = 1)
    {
        var item = GetItem(itemName);
        if (item == null) return;
        
        // Create dropped item in world (implement based on your needs)
        CreateDroppedItem(item, quantity);
        RemoveItem(itemName, quantity);
    }
    
    private void CreateDroppedItem(InventoryItem item, int quantity)
    {
        // This would create a physical item in the world
        // Implementation depends on your game's item system
        Debug.Log($"Dropped {quantity}x {item.itemName}");
    }
    
    public void ClearInventory()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke();
    }
    
    private void PlayPickupSound()
    {
        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);
    }
    
    private void PlayDropSound()
    {
        if (dropSound != null && audioSource != null)
            audioSource.PlayOneShot(dropSound);
    }
    
    private void PlayErrorSound()
    {
        if (errorSound != null && audioSource != null)
            audioSource.PlayOneShot(errorSound);
    }
    
    // Debug methods
    [ContextMenu("Debug Print Inventory")]
    public void DebugPrintInventory()
    {
        Debug.Log("=== INVENTORY ===");
        Debug.Log($"Used Slots: {GetUsedSlots()}/{maxSlots}");
        Debug.Log($"Weight: {GetCurrentWeight():F1}/{maxWeight}");
        Debug.Log("Items:");
        
        foreach (var item in inventory)
        {
            Debug.Log($"  - {item.itemName} x{item.quantity} (Weight: {item.weight * item.quantity:F1})");
        }
    }
    
    [ContextMenu("Add Test Items")]
    public void AddTestItems()
    {
        AddItem("Flashlight", "A reliable flashlight", null, 1, false, 2f);
        AddItem("Key", "Mysterious old key", null, 1, false, 0.1f);
        AddItem("EMF Reader", "Detects electromagnetic fields", null, 1, false, 1.5f);
        AddItem("Batteries", "AA Batteries", null, 4, true, 0.2f);
        AddItem("Evidence", "Strange photograph", null, 1, true, 0.1f);
    }
}

/// <summary>
/// Example of an advanced interactable that uses the inventory system
/// </summary>
public class ItemPickup : InteractableObject
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private string itemDescription = "A mysterious item";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private int quantity = 1;
    [SerializeField] private bool stackable = true;
    [SerializeField] private float weight = 1f;
    
    [Header("Spawn Settings")]
    [SerializeField] private bool randomizeOnStart = false;
    [SerializeField] private string[] possibleItems = { "Key", "Evidence", "Batteries", "Notebook" };
    
    private InventoryManager inventoryManager;
    
    protected override void Start()
    {
        base.Start();
        inventoryManager = FindFirstObjectByType<InventoryManager>();
        
        if (randomizeOnStart && possibleItems.Length > 0)
        {
            itemName = possibleItems[Random.Range(0, possibleItems.Length)];
        }
    }
    
    protected override void OnInteract(Playercontroller player)
    {
        if (inventoryManager != null)
        {
            bool success = inventoryManager.AddItem(itemName, itemDescription, itemIcon, quantity, stackable, weight);
            
            if (success)
            {
                // Destroy the pickup object
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning("No InventoryManager found in scene!");
        }
    }
    
    public override string GetInteractionText()
    {
        return $"Pick up {itemName}";
    }
}