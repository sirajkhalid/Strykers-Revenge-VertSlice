using UnityEngine;
using TMPro;

public class TMPGlowEffect : MonoBehaviour
{
    public TMP_Text tmp;
    public Color outlineColor = Color.white;
    public float outlineWidth = 0.25f;

    public Color glowColor = new Color(1f, 1f, 1f, 0.5f);
    public float glowPower = 0.75f;

    private Material mat;

    void Awake()
    {
        if (tmp == null)
            tmp = GetComponent<TMP_Text>();

        mat = Instantiate(tmp.fontMaterial);
        tmp.fontMaterial = mat;

        ApplyEffect();
    }

    void ApplyEffect()
    {
        // OUTLINE
        mat.SetFloat("_OutlineWidth", outlineWidth);
        mat.SetColor("_OutlineColor", outlineColor);

        // GLOW
        if (mat.HasProperty("_GlowColor"))
        {
            mat.SetColor("_GlowColor", glowColor);
            mat.SetFloat("_GlowPower", glowPower);
        }
    }
}
