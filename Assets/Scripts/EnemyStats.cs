using UnityEngine;
using System;

public enum CreatureType
{
    Aberration,
    Beast,
    Celestial,
    Construct,
    Dragon,
    Elemental,
    Fey,
    Fiend,
    Giant,
    Humanoid,
    Monstrosity,
    Ooze,
    Plant,
    Undead,
    Goblinoid,
    Demon,
    Devil,
    Shapechanger,
    Insect,
    Mechanical,
    Spirit,
    MagicalBeast,
    Unknown
}

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : MonoBehaviour
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public CreatureType creatureType = CreatureType.Unknown;
    public Sprite enemyPortrait;
    public int level = 1;

    [Header("Primary Attributes")]
    public int strength = 10;
    public int dexterity = 10;
    public int constitution = 10;
    public int intelligence = 10;
    public int wisdom = 10;
    public int charisma = 10;

    [Header("Auto-Calculated Modifiers (read-only)")]
    [SerializeField] private int strMod;
    [SerializeField] private int dexMod;
    [SerializeField] private int conMod;
    [SerializeField] private int intMod;
    [SerializeField] private int wisMod;
    [SerializeField] private int chaMod;

    [Header("Combat Stats")]
    public int baseHealth = 10;
    public int maxHealth;
    [HideInInspector] public int currentHealth;
    public int armorClass = 10;
    public int initiative = 0;
    public int xpReward = 50;

    [Header("Abilities")]
    [SerializeReference]
    public BaseEnemyAbilities creatureAbilities;


    // event for health changes
    public event Action OnHealthChanged;

    void Awake()
    {
        CalculateModifiers();
        CalculateHealth();
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    void CalculateModifiers()
    {
        strMod = Mathf.FloorToInt((strength - 10) / 2f);
        dexMod = Mathf.FloorToInt((dexterity - 10) / 2f);
        conMod = Mathf.FloorToInt((constitution - 10) / 2f);
        intMod = Mathf.FloorToInt((intelligence - 10) / 2f);
        wisMod = Mathf.FloorToInt((wisdom - 10) / 2f);
        chaMod = Mathf.FloorToInt((charisma - 10) / 2f);
    }

    void CalculateHealth()
    {
        int baseHP = baseHealth + (conMod * 2);
        maxHealth = Mathf.Max(baseHP + (level * 2), 1);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        CalculateModifiers();
        CalculateHealth();
    }
#endif

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }
}
