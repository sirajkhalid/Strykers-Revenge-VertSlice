using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class BannerAnimator : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textMesh;  

    [Header("Show Animation")]
    public float fadeInDuration = 0.25f;
    public float scaleUpAmount = 1.15f;
    public float scaleUpDuration = 0.25f;

    [Header("Flash Effect")]
    public bool useFlash = true;
    public Color flashColor = new Color(0.9f, 0.9f, 1f);
    public float flashDuration = 0.15f;

    [Header("Shake Effect")]
    public bool useShake = true;
    public float shakeDuration = 0.3f;
    public float shakeStrength = 12f;
    public int shakeVibrato = 10;

    [Header("Timing")]
    public float holdTime = 0.8f;

    [Header("Hide Animation")]
    public float fadeOutDuration = 0.4f;
    public float slideAmount = 20f;

    private CanvasGroup canvasGroup;
    private RectTransform rect;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        if (textMesh == null)
            textMesh = GetComponentInChildren<TextMeshProUGUI>();

        // Start disabled
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void PlayBanner(string message)
    {
        if (textMesh == null)
        {
            Debug.LogWarning("BannerAnimator has no assigned TextMeshProUGUI!");
            return;
        }

        DOTween.Kill(rect);
        DOTween.Kill(canvasGroup);

        textMesh.text = message;

        gameObject.SetActive(true);     

        // Reset state
        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.zero;
        rect.anchoredPosition = Vector2.zero;
        textMesh.color = Color.white;

        Sequence seq = DOTween.Sequence();

        // Fade + pop
        seq.Append(canvasGroup.DOFade(1f, fadeInDuration));
        seq.Join(rect.DOScale(scaleUpAmount, scaleUpDuration).SetEase(Ease.OutBack));

        // Settle
        seq.Append(rect.DOScale(1f, 0.18f));

        // Flash
        if (useFlash)
        {
            seq.Join(textMesh.DOColor(flashColor, flashDuration));
            seq.Append(textMesh.DOColor(Color.white, flashDuration));
        }

        // Shake
        if (useShake)
        {
            seq.Append(rect.DOShakeAnchorPos(
                shakeDuration,
                shakeStrength,
                shakeVibrato,
                90,
                false,
                true));
        }

        
        seq.AppendInterval(holdTime);

        
        seq.Append(canvasGroup.DOFade(0f, fadeOutDuration));
        seq.Join(rect.DOAnchorPosY(slideAmount, fadeOutDuration).SetEase(Ease.InOutSine));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        seq.Play();
    }
}
