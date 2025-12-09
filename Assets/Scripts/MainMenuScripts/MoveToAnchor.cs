using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MoveToAnchor : MonoBehaviour
{
    public Transform anchor;                
    public float delay = 0.1f;
    public float duration = 0.6f;
    public AnimationCurve curve;             
    public bool useUnscaledTime = true;

    [Header("Events")]
    public UnityEvent onArrive;              

    Vector3 startPos;

    void Awake()
    {
        startPos = transform.position;
    }

    IEnumerator Start()
    {
        if (useUnscaledTime && delay > 0f) yield return new WaitForSecondsRealtime(delay);
        else if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = (curve != null && curve.length > 0) ? curve.Evaluate(p) : p;
            transform.position = Vector3.Lerp(startPos, anchor.position, eased);
            yield return null;
        }

        transform.position = anchor.position;
        onArrive?.Invoke();                  // <- triggers flames (and anything else)
    }
}
