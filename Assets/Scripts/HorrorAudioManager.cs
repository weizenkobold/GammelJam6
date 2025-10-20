using UnityEngine;
using System.Collections;

/// <summary>
/// Manages horror audio effects and ambient sounds
/// </summary>
public class HorrorAudioManager : MonoBehaviour
{
    [Header("Ambient Sounds")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip[] ambientClips;
    [SerializeField] private float ambientVolume = 0.3f;
    [SerializeField] private Vector2 ambientIntervalRange = new Vector2(5f, 15f);
    
    [Header("Scare Sounds")]
    [SerializeField] private AudioSource scareSource;
    [SerializeField] private AudioClip[] scareClips;
    [SerializeField] private AudioClip[] whisperClips;
    [SerializeField] private AudioClip[] footstepScareClips;
    [SerializeField] private float scareVolume = 0.8f;
    
    [Header("Heartbeat")]
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private AudioClip heartbeatClip;
    [SerializeField] private float maxHeartbeatVolume = 0.6f;
    
    [Header("3D Audio Settings")]
    [SerializeField] private float maxAudioDistance = 20f;
    [SerializeField] private AnimationCurve audioFalloffCurve;
    
    private Playercontroller player;
    private Coroutine ambientCoroutine;
    private bool isPlayingHeartbeat = false;
    
    void Start()
    {
        player = FindFirstObjectByType<Playercontroller>();
        
        // Setup audio sources
        SetupAudioSource(ambientSource);
        SetupAudioSource(scareSource);
        SetupAudioSource(heartbeatSource);
        
        // Start ambient audio loop
        if (ambientClips.Length > 0)
            ambientCoroutine = StartCoroutine(AmbientAudioLoop());
    }
    
    void Update()
    {
        if (player != null)
        {
            UpdateHeartbeat();
        }
    }
    
    void SetupAudioSource(AudioSource source)
    {
        if (source != null)
        {
            source.spatialBlend = 0f; // 2D audio by default
            source.rolloffMode = AudioRolloffMode.Custom;
            source.maxDistance = maxAudioDistance;
        }
    }
    
    void UpdateHeartbeat()
    {
        float fearLevel = player.GetFearLevel();
        
        if (fearLevel > 0.3f && !isPlayingHeartbeat)
        {
            StartHeartbeat();
        }
        else if (fearLevel <= 0.1f && isPlayingHeartbeat)
        {
            StopHeartbeat();
        }
        
        if (isPlayingHeartbeat && heartbeatSource != null)
        {
            float targetVolume = Mathf.Lerp(0f, maxHeartbeatVolume, fearLevel);
            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, targetVolume, Time.deltaTime * 2f);
            
            // Increase heartbeat speed with fear
            float targetPitch = Mathf.Lerp(1f, 1.5f, fearLevel);
            heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, targetPitch, Time.deltaTime);
        }
    }
    
    void StartHeartbeat()
    {
        if (heartbeatSource != null && heartbeatClip != null)
        {
            heartbeatSource.clip = heartbeatClip;
            heartbeatSource.loop = true;
            heartbeatSource.Play();
            isPlayingHeartbeat = true;
        }
    }
    
    void StopHeartbeat()
    {
        if (heartbeatSource != null)
        {
            StartCoroutine(FadeOutHeartbeat());
        }
    }
    
    IEnumerator FadeOutHeartbeat()
    {
        float startVolume = heartbeatSource.volume;
        float elapsed = 0f;
        float fadeTime = 2f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            heartbeatSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }
        
        heartbeatSource.Stop();
        isPlayingHeartbeat = false;
    }
    
    IEnumerator AmbientAudioLoop()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = Random.Range(ambientIntervalRange.x, ambientIntervalRange.y);
            yield return new WaitForSeconds(waitTime);
            
            // Play random ambient sound
            if (ambientClips.Length > 0 && ambientSource != null)
            {
                AudioClip clipToPlay = ambientClips[Random.Range(0, ambientClips.Length)];
                ambientSource.PlayOneShot(clipToPlay, ambientVolume);
            }
        }
    }
    
    public void PlayScareSound()
    {
        if (scareClips.Length > 0 && scareSource != null)
        {
            AudioClip clipToPlay = scareClips[Random.Range(0, scareClips.Length)];
            scareSource.PlayOneShot(clipToPlay, scareVolume);
            
            // Add fear to player
            if (player != null)
                player.AddFear(20f);
        }
    }
    
    public void PlayWhisper()
    {
        if (whisperClips.Length > 0 && scareSource != null)
        {
            AudioClip clipToPlay = whisperClips[Random.Range(0, whisperClips.Length)];
            scareSource.PlayOneShot(clipToPlay, scareVolume * 0.5f);
            
            // Add moderate fear
            if (player != null)
                player.AddFear(10f);
        }
    }
    
    public void PlayFootstepScare()
    {
        if (footstepScareClips.Length > 0 && scareSource != null)
        {
            AudioClip clipToPlay = footstepScareClips[Random.Range(0, footstepScareClips.Length)];
            scareSource.PlayOneShot(clipToPlay, scareVolume * 0.7f);
            
            // Add slight fear
            if (player != null)
                player.AddFear(5f);
        }
    }
    
    public void Play3DAudioAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null && player != null)
        {
            // Create temporary audio source
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = position;
            
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = volume;
            tempSource.spatialBlend = 1f; // 3D audio
            tempSource.rolloffMode = AudioRolloffMode.Custom;
            tempSource.maxDistance = maxAudioDistance;
            tempSource.Play();
            
            // Destroy after clip finishes
            Destroy(tempAudio, clip.length);
            
            // Calculate fear based on distance
            float distance = Vector3.Distance(position, player.transform.position);
            float fearAmount = Mathf.Lerp(15f, 5f, distance / maxAudioDistance);
            player.AddFear(fearAmount);
        }
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
            ambientSource.volume = ambientVolume;
    }
    
    public void SetScareVolume(float volume)
    {
        scareVolume = Mathf.Clamp01(volume);
        if (scareSource != null)
            scareSource.volume = scareVolume;
    }
    
    void OnDestroy()
    {
        if (ambientCoroutine != null)
            StopCoroutine(ambientCoroutine);
    }
}