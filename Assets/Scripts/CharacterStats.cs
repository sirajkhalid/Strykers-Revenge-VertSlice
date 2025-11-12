using UnityEngine;
using System;

public enum Race
{
    Human,
    Elf,
    Dwarf,
    Hellspawn,
    DragonHybrid
}

public enum CharacterClass
{
    Fighter,
    Barbarian,
    Wizard,
    Cleric,
    Ranger,
    Rogue
}

public enum Background
{
    Acolyte,
    Charlatan,
    Criminal,
    Entertainer,
    FolkHero,
    GuildArtisan,
    Noble,
    Outlander,
    Sage,
    Soldier,
    Urchin
}

public enum Alignment
{
    LawfulGood,
    NeutralGood,
    ChaoticGood,
    LawfulNeutral,
    TrueNeutral,
    ChaoticNeutral,
    LawfulEvil,
    NeutralEvil,
    ChaoticEvil
}

public class CharacterStats : MonoBehaviour
{
    [Header("Basic Info")]
    public string characterName = "Player";
    public Race race = Race.Human;
    public CharacterClass characterClass = CharacterClass.Fighter;
    public Background background = Background.FolkHero;
    public Alignment alignment = Alignment.TrueNeutral;

    [Header("Portrait")]
    public Sprite characterPortrait;

    [Header("Progression")]
    public int level = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 1000;

    [Header("Primary Attributes (Editable)")]
    public int strength = 10;
    public int dexterity = 10;
    public int constitution = 10;
    public int intelligence = 10;
    public int wisdom = 10;
    public int charisma = 10;

    [Header("Modifiers (auto)")]
    [SerializeField] private int strMod;
    [SerializeField] private int dexMod;
    [SerializeField] private int conMod;
    [SerializeField] private int intMod;
    [SerializeField] private int wisMod;
    [SerializeField] private int chaMod;

    [Header("Combat Stats (auto)")]
    public int maxHealth;
    public int currentHealth;
    public int armorClass;
    public int initiative;

    [Header("Skills (auto, flat numbers)")]
    public int athletics;
    public int acrobatics;
    public int sleightOfHand;
    public int stealth;
    public int arcana;
    public int history;
    public int investigation;
    public int nature;
    public int religion;
    public int animalHandling;
    public int insight;
    public int medicine;
    public int perception;
    public int survival;
    public int deception;
    public int intimidation;
    public int performance;
    public int persuasion;

    [Header("Movement")]
    public float maxMovement = 12f;    // Default out-of-combat movement
    public float currentMovement;      // Used during battle

    [Header("UI Feedback")]
    public GameObject floatingDamagePrefab;
    public GameObject outOfRangePrefab;

    public event Action OnHealthChanged; //Called when health changes
    public event Action OnStatsInitialized;
    public event Action OnMovementChanged;

    [Header("Spell Slots")]
    public int maxLevel1Slots;
    public int currentLevel1Slots;

    public int maxLevel2Slots;
    public int currentLevel2Slots;

    [Header("Action Economy")]
    public bool hasAction = true;
    public bool hasBonusAction = true;


    void Start()
    {
        CalculateAllStats();
        CalculateSpellSlots();
        
        currentHealth = maxHealth;
        currentMovement = maxMovement;

        OnStatsInitialized?.Invoke();
        OnHealthChanged?.Invoke();
        OnMovementChanged?.Invoke();

    }

    public void CalculateAllStats()
    {
        CalculateModifiers();
        ApplyRaceBonuses();
        ApplyClassBonuses();
        CalculateDerivedStats();
        CalculateSkills();
        CalculateSpellSlots();
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

    void ApplyRaceBonuses()
    {
        switch (race)
        {
            case Race.Human:
                strength += 1; dexterity += 1; constitution += 1;
                intelligence += 1; wisdom += 1; charisma += 1;
                break;
            case Race.Elf:
                dexterity += 2; perception += 1;
                break;
            case Race.Dwarf:
                constitution += 2; wisdom += 1;
                break;
            case Race.Hellspawn:
                charisma += 2; intelligence += 1;
                break;
            case Race.DragonHybrid:
                strength += 2; charisma += 1;
                break;
        }
    }

    void ApplyClassBonuses()
    {
        switch (characterClass)
        {
            case CharacterClass.Rogue:
                sleightOfHand += 1;
                stealth += 1;
                break;
            case CharacterClass.Ranger:
                survival += 1;
                perception += 1;
                break;
            case CharacterClass.Fighter:
                athletics += 1;
                intimidation += 1;
                break;
            case CharacterClass.Barbarian:
                athletics += 1;
                survival += 1;
                break;
            case CharacterClass.Wizard:
                arcana += 1;
                history += 1;
                break;
            case CharacterClass.Cleric:
                insight += 1;
                religion += 1;
                break;
        }
    }

    void CalculateDerivedStats()
    {
        // Determine class-based HP growth
        int baseHP = 0;
        int hpPerLevel = 0;

        switch (characterClass)
        {
            case CharacterClass.Barbarian:
                baseHP = 12; hpPerLevel = 7; break;
            case CharacterClass.Fighter:
            case CharacterClass.Ranger:
                baseHP = 10; hpPerLevel = 6; break;
            case CharacterClass.Cleric:
            case CharacterClass.Rogue:
                baseHP = 8; hpPerLevel = 5; break;
            case CharacterClass.Wizard:
                baseHP = 6; hpPerLevel = 4; break;
            default:
                baseHP = 8; hpPerLevel = 5; break;
        }

        // Starting HP + HP per level scaling
        maxHealth = baseHP + conMod;
        if (level > 1)
            maxHealth += (level - 1) * (hpPerLevel + conMod);

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0) currentHealth = maxHealth;

        // Initiative & Armor Class
        initiative = dexMod;
        armorClass = characterClass == CharacterClass.Barbarian
            ? 10 + dexMod + conMod
            : 10 + dexMod;

        // EXP Scaling
        expToNextLevel = level * 1000;
    }

    void CalculateSpellSlots()
    {
        // Default: no spell slots
        maxLevel1Slots = 0;
        maxLevel2Slots = 0;

        switch (characterClass)
        {
            case CharacterClass.Wizard:
            case CharacterClass.Cleric:
                if (level <= 1)
                {
                    maxLevel1Slots = 2;
                }
                else if (level <= 3)
                {
                    maxLevel1Slots = 3;
                }
                else if (level <= 5)
                {
                    maxLevel1Slots = 4;
                    maxLevel2Slots = 2;
                }
                else
                {
                    maxLevel1Slots = 4;
                    maxLevel2Slots = 3;
                }
                break;

            case CharacterClass.Ranger:
                if (level <= 1)
                {
                    maxLevel1Slots = 1;
                }
                else if (level <= 3)
                {
                    maxLevel1Slots = 2;
                }
                else if (level <= 5)
                {
                    maxLevel1Slots = 2;
                    maxLevel2Slots = 1;
                }
                else
                {
                    maxLevel1Slots = 3;
                    maxLevel2Slots = 1;
                }
                break;

            // Fighter, Barbarian, Rogue – no spell slots
            case CharacterClass.Fighter:
            case CharacterClass.Barbarian:
            case CharacterClass.Rogue:
            default:
                maxLevel1Slots = 0;
                maxLevel2Slots = 0;
                break;
        }

        // If current slots exceed max (testing purposes)
        currentLevel1Slots = Mathf.Clamp(currentLevel1Slots, 0, maxLevel1Slots);
        currentLevel2Slots = Mathf.Clamp(currentLevel2Slots, 0, maxLevel2Slots);

        // If initializing (first time), ensure full recharge
        if (currentLevel1Slots == 0 && level == 1)
            currentLevel1Slots = maxLevel1Slots;
        if (currentLevel2Slots == 0 && level == 1)
            currentLevel2Slots = maxLevel2Slots;
    }


    void CalculateSkills()
    {
        // Strength
        athletics = 10 + strMod;
        // Dexterity
        acrobatics = 10 + dexMod;
        sleightOfHand = 10 + dexMod;
        stealth = 10 + dexMod;
        // Intelligence
        arcana = 10 + intMod;
        history = 10 + intMod;
        investigation = 10 + intMod;
        nature = 10 + intMod;
        religion = 10 + intMod;
        // Wisdom
        animalHandling = 10 + wisMod;
        insight = 10 + wisMod;
        medicine = 10 + wisMod;
        perception = 10 + wisMod;
        survival = 10 + wisMod;
        // Charisma
        deception = 10 + chaMod;
        intimidation = 10 + chaMod;
        performance = 10 + chaMod;
        persuasion = 10 + chaMod;
    }

    //  Leveling System 
    public void GainEXP(int amount)
    {
        currentEXP += amount;
        if (currentEXP >= expToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        level++;
        currentEXP -= expToNextLevel;
        expToNextLevel = level * 1000;

        // Recalculate HP 
        int hpPerLevel = 0;
        switch (characterClass)
        {
            case CharacterClass.Barbarian: hpPerLevel = 7; break;
            case CharacterClass.Fighter:
            case CharacterClass.Ranger: hpPerLevel = 6; break;
            case CharacterClass.Cleric:
            case CharacterClass.Rogue: hpPerLevel = 5; break;
            case CharacterClass.Wizard: hpPerLevel = 4; break;
        }

        int hpGain = hpPerLevel + conMod;
        maxHealth += hpGain;
        currentHealth = maxHealth;

        CalculateAllStats();
        CalculateSpellSlots();

        Debug.Log($"{characterName} leveled up to {level}! HP increased by {hpGain}.");
        
    }

    // Utility Functions for UI / External Scripts
    public void AddAttributePoint(string attributeName)
    {
        attributeName = attributeName.ToLower();
        switch (attributeName)
        {
            case "strength": strength++; break;
            case "dexterity": dexterity++; break;
            case "constitution": constitution++; break;
            case "intelligence": intelligence++; break;
            case "wisdom": wisdom++; break;
            case "charisma": charisma++; break;
            default: Debug.LogWarning("Invalid attribute name."); return;
        }

        CalculateAllStats();
    }

    public void AddSkillPoint(string skillName)
    {
        skillName = skillName.ToLower();
        switch (skillName)
        {
            case "athletics": athletics++; break;
            case "acrobatics": acrobatics++; break;
            case "sleightofhand": sleightOfHand++; break;
            case "stealth": stealth++; break;
            case "arcana": arcana++; break;
            case "history": history++; break;
            case "investigation": investigation++; break;
            case "nature": nature++; break;
            case "religion": religion++; break;
            case "animalhandling": animalHandling++; break;
            case "insight": insight++; break;
            case "medicine": medicine++; break;
            case "perception": perception++; break;
            case "survival": survival++; break;
            case "deception": deception++; break;
            case "intimidation": intimidation++; break;
            case "performance": performance++; break;
            case "persuasion": persuasion++; break;
            default: Debug.LogWarning("Invalid skill name."); return;
        }

        Debug.Log($"Increased {skillName} by 1 point.");
    }

    public void SetCurrentHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void SetCurrentMovement(float newValue)
    {
        currentMovement = Mathf.Clamp(newValue, 0, maxMovement);
        OnMovementChanged?.Invoke();
    }

    public void ResetMovement()
    {
        currentMovement = maxMovement;
        OnMovementChanged?.Invoke();
    }

    public void ShowFloatingText(string text, Color color)
    {
        if (floatingDamagePrefab == null) return;
        Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
        GameObject popup = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
        popup.GetComponent<FloatingDamage>().SetText(text, color);
    }

    public bool CanUseActionType(Ability.ActionType type)
    {
        if (type == Ability.ActionType.Action) return hasAction;
        if (type == Ability.ActionType.BonusAction) return hasBonusAction;
        return true; // FreeAction
    }

    public void ConsumeActionType(Ability.ActionType type)
    {
        if (type == Ability.ActionType.Action) hasAction = false;
        else if (type == Ability.ActionType.BonusAction) hasBonusAction = false;
    }
    public bool HasSpellSlots(int spellLevel, int cost)
    {
        if (cost <= 0 || spellLevel <= 0) return true; // cantrips or free
        if (spellLevel == 1) return currentLevel1Slots >= cost;
        if (spellLevel == 2) return currentLevel2Slots >= cost;
        return false;
    }

    public void SpendSpellSlots(int spellLevel, int cost)
    {
        if (cost <= 0 || spellLevel <= 0) return;
        if (spellLevel == 1) currentLevel1Slots = Mathf.Max(0, currentLevel1Slots - cost);
        else if (spellLevel == 2) currentLevel2Slots = Mathf.Max(0, currentLevel2Slots - cost);
    }

    // Reset each time this character’s turn starts
    public void ResetTurnActions()
    {
        hasAction = true;
        hasBonusAction = true;
    }

}
