using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class PartyPortraitFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    public float hoverScale = 1.12f;
    public float hoverTime = 0.15f;
    public float selectPunch = 0.15f;

    [Header("References")]
    public Image portraitImage;
    public TMP_Text hpText;

    private RectTransform rect;
    private Tween scaleTween;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (portraitImage == null)
            portraitImage = GetComponent<Image>();
    }

    // Hover Animation
    public void OnPointerEnter(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = rect
            .DOScale(hoverScale, hoverTime)
            .SetEase(Ease.OutBack);

        hpText?.DOFade(1f, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = rect
            .DOScale(1f, hoverTime)
            .SetEase(Ease.OutBack);
    }

    // Selection Animation
    public void PlaySelectEffect()
    {
        rect.DOPunchScale(Vector3.one * selectPunch, 0.25f, 8, 0.8f);
        portraitImage.DOColor(Color.white, 0.2f);
    }

    // Dead Effect
    public void PlayDeadEffect()
    {
        portraitImage.DOFade(0.3f, 0.3f);
        rect.DOScale(0.9f, 0.25f);
    }

    public void ResetToNormal()
    {
        portraitImage.DOFade(1f, 0.2f);
        rect.DOScale(1f, 0.2f);
    }
}
