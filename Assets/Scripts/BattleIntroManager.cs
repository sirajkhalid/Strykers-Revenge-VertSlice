using UnityEngine;
using TMPro;
using DG.Tweening; // Required for DOTween

public class BattleIntroManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI battleStartText;

    [Header("Animation Settings")]
    public float scaleUpAmount = 1.2f;      // How large it grows when appearing
    public float scaleUpDuration = 0.3f;    // Speed of the scale-up pop
    public float shakeDuration = 0.5f;      // How long it shakes
    public float shakeStrength = 15f;       // Intensity of the shake
    public int shakeVibrato = 10;           // Number of shakes during duration
    public float visibleTime = 1.0f;        // Time before fading
    public float fadeDuration = 0.8f;       // Fade-out time

    private CanvasGroup canvasGroup;
    private RectTransform rect;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rect = battleStartText.rectTransform;
    }

    void Start()
    {
        canvasGroup.alpha = 0f;
        if (battleStartText != null)
            battleStartText.enabled = true;
    }

    public void PlayBattleIntro()
    {
        if (canvasGroup == null || battleStartText == null)
            return;

        // Stop any ongoing tweens
        DOTween.Kill(rect);
        DOTween.Kill(canvasGroup);

        // Reset state
        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one;

        // Build sequence
        Sequence seq = DOTween.Sequence();


        seq.Append(canvasGroup.DOFade(1f, 0.2f));
        seq.Join(rect.DOScale(scaleUpAmount, scaleUpDuration).SetEase(Ease.OutBack));


        seq.Append(rect.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato, 90, false, true));


        seq.AppendInterval(visibleTime);


        seq.Append(canvasGroup.DOFade(0f, fadeDuration));
        seq.Join(rect.DOScale(1f, fadeDuration).SetEase(Ease.InOutSine));


        seq.OnComplete(() =>
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        });


        seq.Play();
    }
}
