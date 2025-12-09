using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Outline))]
public class HoverGlowOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Glow Animation Settings")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;
    public float glowDuration = 0.8f;

    [Header("Thickness Growth")]
    public float baseThickness = 2f;
    public float maxThickness = 12f;
    public float thicknessGrowSpeed = 2f;

    private Outline outline;
    private Tween glowTween;
    private Tween thicknessTween;

    private Color categoryColor = Color.white;
    private float currentThickness;

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0);
        outline.effectDistance = new Vector2(baseThickness, baseThickness);
        currentThickness = baseThickness;
    }

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        categoryColor = GetCategoryColor();
        StartGlow();
        StartThicknessGrowth();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopGlow();
        ResetThickness();
    }

    private void StartGlow()
    {
        glowTween?.Kill();

        outline.effectColor = new Color(categoryColor.r, categoryColor.g, categoryColor.b, maxAlpha);

        glowTween = DOTween.Sequence()
            .Append(outline.DOFade(minAlpha, glowDuration))
            .Append(outline.DOFade(maxAlpha, glowDuration))
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StartThicknessGrowth()
    {
        thicknessTween?.Kill();

        thicknessTween = DOTween.To(
            () => currentThickness,
            x =>
            {
                currentThickness = x;
                outline.effectDistance = new Vector2(currentThickness, currentThickness);
            },
            maxThickness,
            thicknessGrowSpeed
        ).SetEase(Ease.OutCubic);
    }

    private void ResetThickness()
    {
        thicknessTween?.Kill();

        currentThickness = baseThickness;
        outline.effectDistance = new Vector2(baseThickness, baseThickness);
    }

    private void StopGlow()
    {
        glowTween?.Kill();
        outline.DOFade(0f, 0.25f);
    }

    void OnDisable()
    {
        glowTween?.Kill();
        thicknessTween?.Kill();
        outline.effectDistance = new Vector2(baseThickness, baseThickness);
        outline.effectColor = new Color(1, 1, 1, 0);
    }
}
