using UnityEngine;

[CreateAssetMenu(fileName = "AIBehaviorProfile", menuName = "AI/Behavior Profile")]
public class AIBehaviorProfile : ScriptableObject
{
    [Header("Positioning")]
    public float preferredMeleeRange = 1.5f;
    public float preferredRangedRange = 6f;

    [Header("Ability Preferences")]
    public bool prefersRanged = false;
    public bool prefersUtilityWhenLowHP = true;

    [Range(0f, 1f)]
    public float utilityHPThreshold = 0.4f;

    [Header("Shadow Step Logic")]
    public bool useShadowStepWhenStuck = true;
    public bool useShadowStepForFlanking = false;
}
