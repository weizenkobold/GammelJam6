using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the player UI including stamina bar, interaction prompts, and fear effects
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas playerCanvas;
    [SerializeField] private Image staminaBar;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private Image crosshair;
    [SerializeField] private Image fearOverlay;
    
    [Header("Stamina Bar Settings")]
    [SerializeField] private Color staminaFullColor = Color.green;
    [SerializeField] private Color staminaLowColor = Color.red;
    [SerializeField] private float staminaLowThreshold = 0.2f;
    
    [Header("Fear Effects")]
    [SerializeField] private Color fearOverlayColor = new Color(1, 0, 0, 0.1f);
    [SerializeField] private float fearPulseSpeed = 2f;
    
    [Header("Crosshair Settings")]
    [SerializeField] private Color normalCrosshairColor = Color.white;
    [SerializeField] private Color interactCrosshairColor = Color.yellow;
    
    private Playercontroller playerController;
    private bool showingInteraction = false;
    private float fearPulseTimer = 0f;
    
    void Start()
    {
        // Find player controller
        playerController = FindFirstObjectByType<Playercontroller>();
        
        if (playerController == null)
        {
            Debug.LogError("PlayerUI: No Playercontroller found in scene!");
            return;
        }
        
        // Initialize UI elements
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
        
        if (fearOverlay != null)
            fearOverlay.color = Color.clear;
    }
    
    void Update()
    {
        if (playerController == null) return;
        
        UpdateStaminaBar();
        UpdateFearEffects();
    }
    
    void UpdateStaminaBar()
    {
        if (staminaBar == null) return;
        
        float staminaPercentage = playerController.GetStaminaPercentage();
        staminaBar.fillAmount = staminaPercentage;
        
        // Change color based on stamina level
        if (staminaPercentage <= staminaLowThreshold)
        {
            staminaBar.color = Color.Lerp(staminaLowColor, staminaFullColor, 
                staminaPercentage / staminaLowThreshold);
        }
        else
        {
            staminaBar.color = staminaFullColor;
        }
    }
    
    void UpdateFearEffects()
    {
        if (fearOverlay == null) return;
        
        float fearLevel = playerController.GetFearLevel();
        
        if (fearLevel > 0)
        {
            fearPulseTimer += Time.deltaTime * fearPulseSpeed;
            float pulse = Mathf.Sin(fearPulseTimer) * 0.5f + 0.5f;
            
            Color overlayColor = fearOverlayColor;
            overlayColor.a = fearLevel * pulse * 0.3f;
            fearOverlay.color = overlayColor;
        }
        else
        {
            fearOverlay.color = Color.clear;
        }
    }
    
    public void ShowInteractionPrompt(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = text;
            interactionText.gameObject.SetActive(true);
            showingInteraction = true;
            
            // Change crosshair color
            if (crosshair != null)
                crosshair.color = interactCrosshairColor;
        }
    }
    
    public void HideInteractionPrompt()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
            showingInteraction = false;
            
            // Reset crosshair color
            if (crosshair != null)
                crosshair.color = normalCrosshairColor;
        }
    }
    
    public void ShowMessage(string message, float duration = 3f)
    {
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }
    
    private System.Collections.IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (interactionText != null)
        {
            string originalText = interactionText.text;
            bool wasActive = interactionText.gameObject.activeSelf;
            
            interactionText.text = message;
            interactionText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(duration);
            
            if (!showingInteraction)
            {
                interactionText.gameObject.SetActive(wasActive);
                interactionText.text = originalText;
            }
        }
    }
    
    public void SetCrosshairVisible(bool visible)
    {
        if (crosshair != null)
            crosshair.gameObject.SetActive(visible);
    }
}