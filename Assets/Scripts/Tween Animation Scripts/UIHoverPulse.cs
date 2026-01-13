using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Pulse Settings")]
    public float pulseScale = 1.1f;
    public float pulseDuration = 0.15f;

    [Header("Audio Settings")]
    public AudioClip hoverSound; // Assign in inspector
    public float volume = 0.45f;

    private AudioSource audioSource;

    void Awake()
    {
        // Auto-add AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = volume;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayPulse();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
        transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
    }

    private void PlayPulse()
    {
        // Play sound
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);

        // Kill existing tweens so effects don’t stack
        transform.DOKill();

        // Hover Pulse Animation
        transform.DOScale(pulseScale, pulseDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad);
    }
}
