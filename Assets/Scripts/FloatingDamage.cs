using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingDamage : MonoBehaviour
{
    [Header("General Settings")]
    public bool useWorldScale = true;     
    public float worldScale = 0.02f;

    [Header("Motion")]
    public float floatDistance = 1.5f;
    public float duration = 0.9f;
    public float horizontalJitter = 0.25f;

    [Header("FX")]
    public bool punchOnSpawn = true;

    private TMP_Text damageText;
    private Vector3 startPos;

    void Awake()
    {
        damageText = GetComponentInChildren<TMP_Text>();
        if (damageText == null)
        {
            Debug.LogWarning("FloatingDamage: No TMP_Text found on prefab.");
            return;
        }

        if (useWorldScale)
            transform.localScale = Vector3.one * worldScale;

        startPos = transform.position;
    }

    public void SetText(string text, Color color)
    {
        if (damageText == null) return;

        damageText.text = text;
        damageText.color = color;

        startPos = transform.position;

        var mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = "UI";
            mr.sortingOrder = 999;
        }

        PlayTween();
    }

    private void PlayTween()
    {
        DOTween.Kill(transform);
        DOTween.Kill(damageText);

        Vector3 targetPos =
            startPos +
            Vector3.up * floatDistance +
            Vector3.right * Random.Range(-horizontalJitter, horizontalJitter);

        Sequence seq = DOTween.Sequence();

        seq.Join(
            transform.DOMove(targetPos, duration)
                     .SetEase(Ease.OutCubic)
        );

        seq.Join(
            damageText.DOFade(0f, duration)
                      .SetEase(Ease.InOutQuad)
        );

        if (punchOnSpawn)
        {
            seq.Join(
                transform.DOPunchScale(
                    new Vector3(0.1f, 0.1f, 0f),
                    0.35f,  
                    8,      
                    0.7f     
                )
            );
        }

        seq.OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
        if (damageText != null)
            DOTween.Kill(damageText);
    }
}
