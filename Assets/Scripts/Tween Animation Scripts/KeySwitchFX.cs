using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class KeySwitchFX : MonoBehaviour
{
    public KeyCode keyCode;
    public float punchAmount = 0.2f;
    public float punchTime = 0.2f;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            rect.DOPunchScale(Vector3.one * punchAmount, punchTime, 8, 0.8f);
        }
    }
}
