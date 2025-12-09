using UnityEngine;
using System.Collections;

public class ImpactJiggle : MonoBehaviour
{
    [Header("Motion")]
    public float posAmplitude = 18f;   // pixels (UI) or world units *  if not UI
    public float frequency = 7f;     // Hz-ish
    public float damping = 4f;     // higher = faster decay
    public float duration = 0.7f;
    public bool useUnscaledTime = true;

    [Header("Squash & Stretch")]
    [Range(0f, 0.5f)] public float squashAmount = 0.18f;

    // cache
    RectTransform rt;
    Vector3 baseWorldPos, baseScale;
    Vector2 baseAnchoredPos;
    bool isUI;
    Coroutine routine;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        isUI = rt != null;

        baseScale = transform.localScale;
        if (isUI) baseAnchoredPos = rt.anchoredPosition;
        else baseWorldPos = transform.position;
    }

    public void Play()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Jiggle());
    }

    IEnumerator Jiggle()
    {
        float t = 0f;
        while (t < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;

            // exponential decay * sine bounce
            float decay = Mathf.Exp(-damping * t);
            float phase = 2f * Mathf.PI * frequency * t;

            float y = posAmplitude * decay * Mathf.Sin(phase);      // starts at 0, then down/up
            float squash = squashAmount * decay * Mathf.Cos(phase); // starts squashed on impact

            // position
            if (isUI) rt.anchoredPosition = baseAnchoredPos + new Vector2(0f, y);
            else transform.position = baseWorldPos + new Vector3(0f, y, 0f);

            // squash & stretch (preserve “volume” feel)
            transform.localScale = new Vector3(
                baseScale.x * (1f + squash),
                baseScale.y * (1f - squash),
                baseScale.z
            );

            yield return null;
        }

        // restore
        if (isUI) rt.anchoredPosition = baseAnchoredPos;
        else transform.position = baseWorldPos;
        transform.localScale = baseScale;
        routine = null;
    }
}
