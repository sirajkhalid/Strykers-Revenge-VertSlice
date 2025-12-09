using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Outline))]
public class GlowOutlineEffect : MonoBehaviour
{
    [Header("Glow Animation Settings")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;
    public float glowDuration = 0.8f;

    [Header("Thickness Growth")]
    public float baseThickness = 2f;
    public float maxThickness = 10f;
    public float thicknessGrowDuration = 1.2f;

    private Outline outline;
    private Tween glowTween;
    private Tween thicknessTween;
    private float currentThickness;

    private Color GetCategoryColor()
    {
        var slot = GetComponent<SkillSlot>();
        if (slot == null || slot.assignedAbility == null)
            return Color.white;

        switch (slot.assignedAbility.category)
        {
            case Ability.AbilityCategory.Melee: return new Color(1f, 0.2f, 0.2f);
            case Ability.AbilityCategory.Ranged: return new Color(1f, 0.85f, 0.2f);
            case Ability.AbilityCategory.Magic: return new Color(0.4f, 0.6f, 1f);
            case Ability.AbilityCategory.Support: return new Color(0.4f, 1f, 0.4f);
            case Ability.AbilityCategory.Utility: return new Color(0.6f, 0.6f, 0.6f);
            case Ability.AbilityCategory.Passive: return new Color(0.75f, 0.5f, 1f);
        }
        return Color.white;
    }

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0);
        outline.effectDistance = new Vector2(baseThickness, baseThickness);
        currentThickness = baseThickness;
    }

    void OnEnable()
    {
        Color c = GetCategoryColor();
        outline.effectColor = new Color(c.r, c.g, c.b, maxAlpha);

        glowTween = DOTween.Sequence()
            .Append(outline.DOFade(minAlpha, glowDuration))
            .Append(outline.DOFade(maxAlpha, glowDuration))
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        thicknessTween = DOTween.To(
            () => currentThickness,
            x =>
            {
                currentThickness = x;
                outline.effectDistance = new Vector2(currentThickness, currentThickness);
            },
            maxThickness,
            thicknessGrowDuration
        ).SetEase(Ease.OutCubic);
    }

    void OnDisable()
    {
        glowTween?.Kill();
        thicknessTween?.Kill();
        outline.effectColor = new Color(1, 1, 1, 0);
        outline.effectDistance = new Vector2(baseThickness, baseThickness);
        currentThickness = baseThickness;
    }
}
