using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Abilities/Ability")]
public class Ability : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    [TextArea(1, 3)] public string abilityDescription;
    public Sprite abilityIcon;
    public bool isUnlocked = true;

    [Header("Damage & Type")]
    public AbilityCategory category = AbilityCategory.Melee;
    public int baseDamage = 0;
    public float damageScaling = 1.0f;
    public string scalingAttribute = "Strength";

    [Header("Resource & Cooldown")]
    public float resourceCost = 0f;
    public float cooldownTime = 0f;

    [Header("Targeting")]
    public TargetType targetType = TargetType.Enemy;
    public float range = 1f;
    public float areaRadius = 0f;
    public bool requiresLineOfSight = true;

    [Header("Visuals & Feedback")]
    public GameObject visualEffectPrefab;
    public AudioClip abilitySound;

    [Header("Special Effects")]
    [Tooltip("Abilities that cause burn, freeze, etc.")]
    public bool appliesStatusEffect = false;
    public string statusEffectName;
    public float statusDuration = 0f;

    [Header("Animation")]
    [Tooltip("The animation clip to play when this ability is used.")]
    public AnimationClip abilityAnimation;

    [Header("Combat Details")]
    public DamageType damageType;
    public SpecialEffect specialEffect;


    public enum AbilityCategory { Melee, Ranged, Magic, Support, Passive }
    public enum TargetType { Self, Enemy, Ally, Area }

    public enum DamageType
    {
        Physical,
        Fire,
        Cold,
        Lightning,
        Poison,
        Holy,
        Shadow
    }

    public enum SpecialEffect
    {
        None,
        Burn,
        Freeze,
        Stun,
        Poisoned,
        Knockback,
        HealOverTime
    }

    public enum ScalingAttribute
    {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma,
        None
    }


}
