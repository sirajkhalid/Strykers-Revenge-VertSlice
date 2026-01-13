using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PauseButtonHoverPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.12f;
    public float hoverTime = 0.15f;

    [Header("Idle Settings")]
    public float normalScale = 1f;
    public float normalTime = 0.18f;

    [Header("Audio Settings")]
    public AudioClip hoverSound;   
    public float volume = 0.45f;

    private RectTransform rect;
    private Tween scaleTween;
    private AudioSource audioSource;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        // Auto-add AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;  // 2D sound
        audioSource.volume = volume;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Play hover sound
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);

        scaleTween?.Kill();
        scaleTween = rect
            .DOScale(hoverScale, hoverTime)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);  // Works when paused
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = rect
            .DOScale(normalScale, normalTime)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);  // Works when paused
    }
}
