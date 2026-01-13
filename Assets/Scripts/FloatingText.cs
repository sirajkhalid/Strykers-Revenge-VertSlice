using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [Header("Animation Settings")]
    public float riseDistance = 1.2f;
    public float duration = 0.9f;
    public float punchScale = 1.3f;
    public float punchDuration = 0.15f;
    public float randomX = 0.35f;

    private TMP_Text text;
    private CanvasGroup group;
    private RectTransform rect;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        rect = GetComponent<RectTransform>();

        group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();

        group.alpha = 1f;
    }

    void Start()
    {
        PlayTween();
    }

    public void SetText(string value, Color color)
    {
        if (text != null)
        {
            text.text = value;
            text.color = color;
        }
    }

    void PlayTween()
    {
        float startX = rect.anchoredPosition.x;
        float drift = Random.Range(-randomX, randomX);

        Vector2 endPos = new Vector2(startX + drift, rect.anchoredPosition.y + riseDistance);

        Sequence seq = DOTween.Sequence();

        seq.Join(rect.DOAnchorPos(endPos, duration).SetEase(Ease.OutCubic));

        seq.Join(group.DOFade(0f, duration));

       seq.Join(rect.DOLocalRotate(
            new Vector3(0, 0, Random.Range(-18f, 18f)),
            duration,
            RotateMode.Fast));

        seq.OnComplete(() => Destroy(gameObject));
    }
}
