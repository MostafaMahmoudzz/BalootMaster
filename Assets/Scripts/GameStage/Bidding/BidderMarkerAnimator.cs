using UnityEngine;

//-------------------------------------------------------
// BidderMarkerAnimator (Optional Enhancement)
//-------------------------------------------------------
// Purpose:
//   Adds smooth animations and visual effects to the current bidder marker.
//   This is an OPTIONAL component you can add to your marker GameObject
//   to make it more visually appealing.
//
// How to use:
//   1. Add this component to your CurrentBidderMarker GameObject
//   2. Configure the animation settings in the Inspector
//   3. The marker will automatically animate when it moves
//-------------------------------------------------------
public class BidderMarkerAnimator : MonoBehaviour
{
    [Header("Movement Animation")]
    [Tooltip("Speed of smooth movement when marker changes position")]
    [Range(1f, 20f)]
    public float moveSpeed = 8f;
    
    [Header("Pulse Animation")]
    [Tooltip("Enable pulsing scale animation")]
    public bool enablePulse = true;
    
    [Tooltip("Speed of the pulse animation")]
    [Range(0.5f, 5f)]
    public float pulseSpeed = 2f;
    
    [Tooltip("Scale range for pulsing (1.0 = normal size)")]
    [Range(0.8f, 1.5f)]
    public float pulseMinScale = 0.9f;
    
    [Range(0.8f, 1.5f)]
    public float pulseMaxScale = 1.1f;
    
    [Header("Rotation Animation")]
    [Tooltip("Enable rotation animation")]
    public bool enableRotation = false;
    
    [Tooltip("Speed of rotation (degrees per second)")]
    [Range(-360f, 360f)]
    public float rotationSpeed = 90f;
    
    [Header("Fade Animation")]
    [Tooltip("Fade in when marker appears")]
    public bool enableFadeIn = true;
    
    [Tooltip("Fade in duration (seconds)")]
    [Range(0.1f, 2f)]
    public float fadeInDuration = 0.3f;
    
    // Private variables
    private Vector3 targetPosition;
    private Vector3 originalScale;
    private float pulseTimer;
    private SpriteRenderer spriteRenderer;
    private float fadeTimer;
    private bool isFadingIn;
    
    //----------------------------------------------
    void Start()
    {
        // Store original scale
        originalScale = transform.localScale;
        
        // Get sprite renderer for fade effects
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Initialize position
        targetPosition = transform.position;
        
        // Start faded out if fade-in is enabled
        if (enableFadeIn && spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
    }
    
    //----------------------------------------------
    void Update()
    {
        // Smooth movement to target position
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                Time.deltaTime * moveSpeed
            );
        }
        
        // Pulse animation
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, 
                (Mathf.Sin(pulseTimer) + 1f) * 0.5f);
            transform.localScale = originalScale * scale;
        }
        
        // Rotation animation
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        
        // Fade in animation
        if (isFadingIn && spriteRenderer != null)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeInDuration);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
            
            if (fadeTimer >= fadeInDuration)
            {
                isFadingIn = false;
            }
        }
    }
    
    //----------------------------------------------
    // Called when the marker's position is updated by BiddingUI
    void OnEnable()
    {
        // Start fade-in when marker becomes active
        if (enableFadeIn)
        {
            isFadingIn = true;
            fadeTimer = 0f;
        }
        
        // Reset pulse timer for smooth animation start
        pulseTimer = 0f;
    }
    
    //----------------------------------------------
    // Public method to set target position (called by BiddingUI if needed)
    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
        
        // Restart fade-in if enabled
        if (enableFadeIn)
        {
            isFadingIn = true;
            fadeTimer = 0f;
        }
    }
    
    //----------------------------------------------
    // Reset marker to default state
    public void ResetMarker()
    {
        transform.localScale = originalScale;
        pulseTimer = 0f;
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }
}

