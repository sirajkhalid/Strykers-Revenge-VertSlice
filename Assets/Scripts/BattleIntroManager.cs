using UnityEngine;
using TMPro;
using DG.Tweening;

public class BattleIntroManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI battleStartText;

    [Header("Animation Settings")]
    public float scaleUpAmount = 1.15f;
    public float scaleDuration = 0.25f;
    public float visibleTime = 1.0f;
    public float fadeDuration = 0.5f;

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
        battleStartText.enabled = true;
    }

    public void PlayBattleIntro()
    {
        DOTween.Kill(rect);
        DOTween.Kill(canvasGroup);

        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();

        // Fade + Scale
        seq.Append(canvasGroup.DOFade(1f, fadeDuration));
        seq.Join(rect.DOScale(scaleUpAmount, scaleDuration).SetEase(Ease.OutBack));

        // Hold
        seq.AppendInterval(visibleTime);

        // Fade out + Reset scale
        seq.Append(canvasGroup.DOFade(0f, fadeDuration));
        seq.Join(rect.DOScale(1f, fadeDuration).SetEase(Ease.InOutSine));
    }
}
