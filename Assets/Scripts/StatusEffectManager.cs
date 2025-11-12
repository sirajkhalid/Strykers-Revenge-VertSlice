using UnityEngine;
using System.Collections;
using DG.Tweening;

public class StatusEffectManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject blessIndicatorPrefab;

    private GameObject activeBlessIndicator;
    private Tween floatTween;
    private Tween fadeTween;

    public void ApplyBless(float duration)
    {
        // Destroy old indicator if it exists
        if (activeBlessIndicator != null)
        {
            floatTween?.Kill();
            fadeTween?.Kill();
            Destroy(activeBlessIndicator);
        }

        // Spawn the indicator above the character
        activeBlessIndicator = Instantiate(blessIndicatorPrefab, transform);
        activeBlessIndicator.transform.localPosition = new Vector3(0, 0.5f, 0);

        // Animate floating using DOTween (gentle up-down loop)
        floatTween = activeBlessIndicator.transform
       .DOLocalMoveY(1f, 1f) // slightly higher for float
       .SetLoops(-1, LoopType.Yoyo)
       .SetEase(Ease.InOutSine);

        // Fade out at the end of duration
        var sr = activeBlessIndicator.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            fadeTween = sr.DOFade(0f, 0.5f)
                .SetDelay(duration - 0.5f)
                .OnComplete(() =>
                {
                    floatTween?.Kill();
                    Destroy(activeBlessIndicator);
                    activeBlessIndicator = null;
                });
        }
        else
        {
            // fallback if no sprite renderer
            StartCoroutine(RemoveBlessAfter(duration));
        }
    }

    private IEnumerator RemoveBlessAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        floatTween?.Kill();
        if (activeBlessIndicator != null)
            Destroy(activeBlessIndicator);
        activeBlessIndicator = null;
    }
}
