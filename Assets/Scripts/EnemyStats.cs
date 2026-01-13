using UnityEngine;
using System;
using System.Collections;


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
    [HideInInspector] 
    public int currentHealth;
    public int armorClass = 10;
    public int initiative = 0;
    public int rolledInitiative = 0;
    public int xpReward = 50;

    [Header("Movement")]
    public float maxMovement = 5f;
    [HideInInspector] public float currentMovement;

    [Header("Animation")]
    public Animator enemyAnimator;
    public string castAnimationTrigger = "CastTrigger";
    public string castAnimationTrigger2 = "CastTrigger2";

    [Header("Spell Slots")]
    public int spellSlotsLevel1 = 0;
    public int spellSlotsLevel2 = 0;

    public int maxSpellSlotsLevel1 = 0;
    public int maxSpellSlotsLevel2 = 0;


    // event for health changes
    public event Action OnHealthChanged;

    void Awake()
    {
        CalculateModifiers();
        CalculateHealth();
        CalculateInitiative();
        CalculateArmorClass();  
        CalculateMovement();

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();
        
        ResetSpellSlots();
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

        // Use Constitution and level to make enemies tankier at higher levels.

        int raceHP = creatureType switch
        {
            CreatureType.Undead => 10,
            CreatureType.Beast => 5,
            CreatureType.Demon => 12,
            CreatureType.Devil => 12,
            CreatureType.Dragon => 20,
            CreatureType.Giant => 18,
            CreatureType.Monstrosity => 12,
            CreatureType.Humanoid => 6,
            _ => 5
        };

        int baseHP = baseHealth + raceHP + (conMod * 3);

        // Scaling per level
        maxHealth = baseHP + (level * (4 + conMod));

        maxHealth = Mathf.Max(1, maxHealth);
    }
    public void CalculateInitiative()
    {
        int dexMod = Mathf.FloorToInt((dexterity - 10) / 2f);
        initiative = dexMod;
    }

    void CalculateArmorClass()
    {
        // Enemy AC depends on type + dexterity
        int typeBase = creatureType switch
        {
            CreatureType.Undead => 12,
            CreatureType.Beast => 11,
            CreatureType.Humanoid => 10,
            CreatureType.Monstrosity => 13,
            CreatureType.Demon => 14,
            CreatureType.Devil => 15,
            CreatureType.Dragon => 16,
            CreatureType.Giant => 12,
            _ => 10
        };

        armorClass = typeBase + Mathf.FloorToInt(dexMod * 0.5f);  // weaker dex scaling than players
    }

    void CalculateMovement()
    {
       
        float raceSpeed = creatureType switch
        {
            CreatureType.Giant => 35f,
            CreatureType.Beast => 40f,
            CreatureType.Dragon => 40f,
            CreatureType.Monstrosity => 35f,
            CreatureType.Undead => 25f,
            CreatureType.Humanoid => 30f,
            _ => 30f
        };

        float movement =
            (raceSpeed / 5f) +      
            (dexMod * 0.2f) +      
            (strength / 20f);      

        maxMovement = Mathf.Max(1f, Mathf.RoundToInt(movement));
        currentMovement = maxMovement;
    }


#if UNITY_EDITOR
    void OnValidate()
    {
        CalculateModifiers();
        CalculateHealth();
    }
#endif

    [Header("Damage Popup")]
    public GameObject floatingDamagePrefab;

    public void TakeDamage(int amount, bool isCrit = false, bool isMiss = false)
    {
        if (isMiss)
        {
            ShowFloatingText("Miss!", Color.white);
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke();

        // Color feedback
        Color textColor = isCrit ? Color.yellow : Color.red;
        ShowFloatingText("-" + amount.ToString(), textColor);

        if (currentHealth <= 0)
            Die();
    }

    public void ShowFloatingText(string text, Color color)
    {
        if (floatingDamagePrefab == null) return;
        Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
        GameObject popup = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
        popup.GetComponent<FloatingDamage>().SetText(text, color);
    }

    private void Die()
    {
        Debug.Log($"{enemyName} has been defeated!");


        currentHealth = 0;

        var ai = GetComponent<EnemyAIController>();
        if (ai != null)
            ai.enabled = false;


        var mover = GetComponent<EnemyMovementController>();
        if (mover != null)
        {
            mover.OnBattleStarted();
            mover.canMove = false;
        }


        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;


        TurnManager turn = FindFirstObjectByType<TurnManager>();
        if (turn != null)
            turn.RemoveCombatant(gameObject);


        BattleStateManager bsm = FindFirstObjectByType<BattleStateManager>();
        if (bsm != null)
            bsm.MarkEnemyKilled(gameObject);


        StartCoroutine(FadeOut());

    }



    private IEnumerator FadeOut()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float fadeDuration = 1.2f;
        float elapsed = 0f;
        Color originalColor = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }


        sr.enabled = false;
    }

    private IEnumerator FadeAndDestroy()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            // No sprite? Just destroy quickly
            Destroy(gameObject, 0.5f);
            yield break;
        }

        float fadeDuration = 1.2f;
        float elapsed = 0f;
        Color originalColor = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

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

    public void ResetTurn()
    {
        currentMovement = maxMovement;
    }

    public bool HasSpellSlots(int level, int cost)
    {
        switch (level)
        {
            case 1: return spellSlotsLevel1 >= cost;
            case 2: return spellSlotsLevel2 >= cost;
            default: return true; 
        }
    }

    public void SpendSpellSlots(int level, int cost)
    {
        switch (level)
        {
            case 1:
                spellSlotsLevel1 = Mathf.Max(0, spellSlotsLevel1 - cost);
                break;
            case 2:
                spellSlotsLevel2 = Mathf.Max(0, spellSlotsLevel2 - cost);
                break;
        }
    }

    public void ResetSpellSlots()
    {
        spellSlotsLevel1 = maxSpellSlotsLevel1;
        spellSlotsLevel2 = maxSpellSlotsLevel2;
    }



}
