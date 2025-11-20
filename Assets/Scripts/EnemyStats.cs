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
        CalculateInitiative();
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
    public void CalculateInitiative()
    {
        int dexMod = Mathf.FloorToInt((dexterity - 10) / 2f);
        initiative = dexMod;
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

        // Disable collider immediately
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Try to fade out if possible
        StartCoroutine(FadeAndDestroy());

        TargetSelector selector = FindFirstObjectByType<TargetSelector>();
        if (selector != null)
            selector.ClearSelection();

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

        Destroy(gameObject);
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
