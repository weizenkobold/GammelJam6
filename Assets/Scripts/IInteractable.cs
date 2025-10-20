using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with by the player
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player interacts with this object
    /// </summary>
    /// <param name="player">Reference to the player controller</param>
    void Interact(Playercontroller player);
    
    /// <summary>
    /// Text to display when the player can interact with this object
    /// </summary>
    string GetInteractionText();
    
    /// <summary>
    /// Whether this object can currently be interacted with
    /// </summary>
    bool CanInteract();
}