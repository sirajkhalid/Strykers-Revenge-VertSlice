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


    [Header("Barrier Effect")]
    public bool hasBarrier = false;
    public int barrierRoundsRemaining = 0;

    [Tooltip("Percentage of damage reduced while barrier is active. 0.5 = 50% reduction.")]
    [Range(0f, 1f)]
    public float barrierDamageReduction = 0.5f;

    private GameObject activeBarrierVFX;



    public void ApplyBarrier(int rounds, float damageReduction, GameObject vfxPrefab)
    {
        hasBarrier = true;
        barrierRoundsRemaining = rounds;
        barrierDamageReduction = damageReduction;


        if (activeBarrierVFX != null)
        {
            Destroy(activeBarrierVFX);
            activeBarrierVFX = null;
        }

        // Spawn VFX on player
        if (vfxPrefab != null)
        {
            activeBarrierVFX = Instantiate(vfxPrefab, transform);
            activeBarrierVFX.transform.localPosition = Vector3.zero;
            activeBarrierVFX.transform.localScale = Vector3.one * 0.5f;


            Destroy(activeBarrierVFX, 2f);
        }
    }

    // Called at the start of each of this character's turns.

    public void TickStatusEffects()
    {
        // Barrier Duration Countdown
        if (hasBarrier)
        {
            barrierRoundsRemaining--;
            if (barrierRoundsRemaining <= 0)
            {
                hasBarrier = false;
            }
        }
    }



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
