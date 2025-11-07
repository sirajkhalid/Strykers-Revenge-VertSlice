using UnityEngine;
using TMPro;

public class FloatingDamage : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float fadeDuration = 1f;
    private TMP_Text damageText;
    private Color originalColor;
    private float elapsed = 0f;

    void Awake()
    {
        damageText = GetComponentInChildren<TMP_Text>();
        originalColor = damageText.color;
    }

    public void SetText(string text, Color color)
    {
        damageText.text = text;
        damageText.color = color;
        transform.localScale = Vector3.one * 0.02f; // Adjust for world space
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        elapsed += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
        damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, alpha);

        if (elapsed >= fadeDuration)
            Destroy(gameObject);
    }
}
