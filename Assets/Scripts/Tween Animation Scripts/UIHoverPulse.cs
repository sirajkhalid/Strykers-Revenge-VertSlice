using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.12f;
    public float hoverTime = 0.15f;

    [Header("Idle Settings")]
    public float normalScale = 1f;
    public float normalTime = 0.18f;

    private RectTransform rect;
    private Tween scaleTween;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = rect
            .DOScale(hoverScale, hoverTime)
            .SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = rect
            .DOScale(normalScale, normalTime)
            .SetEase(Ease.OutBack);
    }
}
