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
    public ScalingAttribute scalingAttribute = ScalingAttribute.Strength;

    [Header("Spell & Resource Settings")]
    [Tooltip("If true, this ability consumes spell slots when used.")]
    public bool usesSpellSlot = true;
    [Range(0, 9)] public int spellLevel = 1;        // 0 = Cantrip
    [Tooltip("Number of spell slots consumed when used.")]
    public int slotCost = 1;

    [Header("Action Economy")]
    public ActionType actionType = ActionType.Action;
    public bool consumesAction = true;
    public bool consumesBonusAction = false;
    public bool consumesFreeAction = false;

    [Header("Targeting")]
    public TargetType targetType = TargetType.Enemy;
    public float range = 1f;
    public float areaRadius = 0f;
    public bool requiresLineOfSight = true;

    [Header("Delivery Type")]
    public DeliveryType deliveryType = DeliveryType.Instant;

    [Header("Visuals & Feedback")]
    public GameObject visualEffectPrefab;
    public AudioClip abilitySound;

    [Header("Special Effects")]
    public bool appliesStatusEffect = false;
    public string statusEffectName;
    public float statusDuration = 0f;

    [Header("Animation")]
    public AnimationClip abilityAnimation;

    [Header("Combat Details")]
    public DamageType damageType;
    public SpecialEffect specialEffect;

    [Header("Damage Dice")]
    public int numberOfDice = 1;
    public int diceSides = 6;

    [Header("Per-Battle Usage")]
    [Tooltip("How many times this ability can be used each battle. 0 = unlimited.")]
    public int maxUsesPerBattle = 0;

    [System.NonSerialized]
    public int usesThisBattle = 0;

    [Header("Lifetime Uses")]
    [Tooltip("How many uses are allowed in the lifetime ")]
    public int MaxLifetimeUses = 0;

    [System.NonSerialized]
    public int LifetimeUses = 0;


    public enum AbilityCategory { Melee, Ranged, Magic, Support, Utility, Passive }
    public enum TargetType { Self, Enemy, Ally, Area }
    public enum DamageType { Physical, Fire, Cold, Lightning, Poison, Holy, Shadow }
    public enum SpecialEffect { None, Burn, Freeze, Stun, Poisoned, Knockback, HealOverTime }
    public enum ScalingAttribute { Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma, None }
    public enum DeliveryType { Instant, Melee, Projectile, Ray, Area, Chain }
    public enum ActionType { Action, BonusAction, FreeAction }

    public bool IsCantrip => !usesSpellSlot && spellLevel == 0;
}
