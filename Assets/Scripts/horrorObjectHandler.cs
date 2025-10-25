using UnityEngine;

public class horrorObjectHandler : MonoBehaviour
{
    [Header("Shaking Settings")]
    [SerializeField] private bool isShaking = false;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeSpeed = 10f;
    [SerializeField] private bool shakeX = true;
    [SerializeField] private bool shakeY = true;
    [SerializeField] private bool shakeZ = true;
    
    
    // Private variables
    private Vector3 originalPosition;
    private float shakeTimer;
    private bool isVanishing;
    void Start()
    {
        // Store the original position
        originalPosition = transform.position;
    }

    void Update()
    {
        HandleShaking();
        if(isVanishing)
        {
            transform.localScale -= Vector3.one * 0.35f * Time.deltaTime;
            if(transform.localScale.x <= 0.01f)
            {
                Destroy(gameObject);
            }
        }
    }
    
    void HandleShaking()
    {
        if (!isShaking) 
        {
            // Return to original position when not shaking
            transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * 2f);
            return;
        }
        
        // Update shake timer
        shakeTimer += Time.deltaTime * shakeSpeed;
        
        // Calculate shake offset
        Vector3 shakeOffset = Vector3.zero;
        
        if (shakeX)
            shakeOffset.x = Mathf.Sin(shakeTimer * 1.1f) * shakeIntensity;
        
        if (shakeY)
            shakeOffset.y = Mathf.Sin(shakeTimer * 0.9f) * shakeIntensity;
        
        if (shakeZ)
            shakeOffset.z = Mathf.Sin(shakeTimer * 1.3f) * shakeIntensity;
        
        // Apply shake to position
        transform.position = originalPosition + shakeOffset;
    }
    
  
    
    // Public methods to control the object
    public void StartShaking()
    {
        isShaking = true;
    }
    
    public void StopShaking()
    {
        isShaking = false;
    }
    
    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }
    
    public void SetShakeSpeed(float speed)
    {
        shakeSpeed = speed;
    }
    
    // Enable/disable specific shake axes
    public void SetShakeAxes(bool x, bool y, bool z)
    {
        shakeX = x;
        shakeY = y;
        shakeZ = z;
    }
    public void vanish()
    {
        isVanishing = true;
    }
}
