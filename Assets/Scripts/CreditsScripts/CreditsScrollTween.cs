using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class CreditScrollTween : MonoBehaviour
{
    [Header("Tween Settings")]
    public float scrollDuration = 20f;     // How long it takes to scroll up
    public float fadeDuration = 2f;        // Fade in/out time
    public float startY = -800f;           // Starting position (off screen bottom)
    public float endY = 800f;              // Ending position (off screen top)

    [Header("Scene")]
    public string mainMenuScene = "MainMenu";

    private CanvasGroup cg;
    private RectTransform rt;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();

        PlayCredits();
    }

    void Update()
    {
        if (Input.anyKeyDown)
            SceneManager.LoadScene(mainMenuScene);
    }

    void PlayCredits()
    {
        // Reset position & alpha
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startY);
        cg.alpha = 0;

        Sequence seq = DOTween.Sequence();

        // Fade in
        seq.Append(cg.DOFade(1f, fadeDuration));

        // Scroll upward
        seq.Append(rt.DOAnchorPosY(endY, scrollDuration).SetEase(Ease.Linear));

        // Fade out
        seq.Append(cg.DOFade(0f, fadeDuration));

        // After it's done → go back to menu
        seq.OnComplete(() =>
        {
            SceneManager.LoadScene(mainMenuScene);
        });
    }
}
