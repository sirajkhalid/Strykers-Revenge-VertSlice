using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class BattleUIManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI turnBannerText;
    public Button endTurnButton;

    [Header("Animation Settings")]
    public float scaleAmount = 1.1f;
    public float fadeDuration = 0.2f;
    public float visibleTime = 0.75f;

    private Coroutine bannerRoutine;
    private TurnManager turnManager;

    private CanvasGroup canvasGroup;
    private RectTransform rect;

    void Awake()
    {
        if (turnBannerText == null)
        {
            Debug.LogError("BattleUIManager: TurnBannerText is not assigned.");
            return;
        }

        canvasGroup = turnBannerText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = turnBannerText.gameObject.AddComponent<CanvasGroup>();

        rect = turnBannerText.rectTransform;
    }

    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>(FindObjectsInactive.Include);

        // Start hidden
        canvasGroup.alpha = 0f;
        turnBannerText.gameObject.SetActive(false);

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnPressed);
    }

    void OnDestroy()
    {
        if (endTurnButton != null)
            endTurnButton.onClick.RemoveListener(OnEndTurnPressed);
    }

    public void ShowTurnBanner(string characterName)
    {
        if (turnBannerText == null) return;

        if (bannerRoutine != null)
            StopCoroutine(bannerRoutine);

        bannerRoutine = StartCoroutine(ShowBannerCoroutine(characterName));
    }

    private IEnumerator ShowBannerCoroutine(string name)
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>(FindObjectsInactive.Include);

        // Determine if this is an enemy turn
        bool isEnemy = false;
        if (turnManager != null && turnManager.currentTurnObject != null)
        {
            isEnemy = turnManager.currentTurnObject.TryGetComponent<EnemyStats>(out _);
        }

        // Player = white, Enemy = red
        turnBannerText.color = isEnemy ? Color.red : Color.white;

        // Prepare the text
        turnBannerText.text = $"{name}'s Turn";
        turnBannerText.gameObject.SetActive(true);

        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one;

        // Kill any old animations
        DOTween.Kill(canvasGroup);
        DOTween.Kill(rect);

        // Animate
        Sequence seq = DOTween.Sequence();

        // Fade + pop
        seq.Append(canvasGroup.DOFade(1f, fadeDuration));
        seq.Join(rect.DOScale(scaleAmount, fadeDuration).SetEase(Ease.OutBack));

        // Hold
        seq.AppendInterval(visibleTime);

        // Fade + reset scale
        seq.Append(canvasGroup.DOFade(0f, fadeDuration));
        seq.Join(rect.DOScale(1f, fadeDuration).SetEase(Ease.InOutSine));

        seq.OnComplete(() =>
        {
            turnBannerText.gameObject.SetActive(false);
        });

        seq.Play();

        yield return seq.WaitForCompletion();
        bannerRoutine = null;
    }

    private void OnEndTurnPressed()
    {
        if (turnManager != null)
            turnManager.EndTurn();
    }
}
