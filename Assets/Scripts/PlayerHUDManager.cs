using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("References")]
    public CharacterStats playerStats;
    public Image playerPortrait;
    public Image healthFill;
    public TMP_Text healthNumText;
    public TMP_Text movementText;

    [Header("Settings")]
    private float maxBarWidth = 0f;
    private RectTransform healthParentRT;
    private bool isInCombat = false;

    [Header("Action Economy UI")]
    public Image actionIcon;
    public Sprite actionAvailableSprite;
    public Sprite actionUsedSprite;

    public Image bonusActionIcon;
    public Sprite bonusAvailableSprite;
    public Sprite bonusUsedSprite;

    [Header("Spell Slot UI")]
    public Transform level1SlotPanel;
    public Transform level2SlotPanel;
    public GameObject spellSlotPrefab;
    public Sprite slotAvailableSprite;
    public Sprite slotUsedSprite;
    public Sprite slotAvailableSpriteII;
    public Sprite slotUsedSpriteII;
    private List<Image> level1Slots = new List<Image>();
    private List<Image> level2Slots = new List<Image>();

    [Header("Spell Slot Panels")]
    [SerializeField] private GameObject spellSlot1Container;
    [SerializeField] private GameObject spellSlot2Container;

    [Header("Resting")]
    public Button shortRestButton;
    private int shortRestsUsed = 0;
    private const int maxShortRests = 3;

    [Header("Skill Bar")]
    public Transform skillBarParent;

    [Header("Animated Health Bar")]
    public Image chipHealthFill;     
    public float chipDelay = 0.25f; 
    public float chipSpeed = 0.6f;   
    public Color damageFlashColor = new Color(1f, 0.3f, 0.3f);
    public float flashDuration = 0.15f;

    private Tween chipTween;
    private Tween flashTween;
    private Tween lowHealthTween;



    void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<CharacterStats>();
    }

    void Start()
    {
        
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnStatsInitialized += InitializeHUD;
            playerStats.OnStatsInitialized += UpdateSpellSlotPanels;
            playerStats.OnMovementChanged += UpdateMovementText;
        }

        
        if (healthFill != null)
            healthParentRT = healthFill.transform.parent.GetComponent<RectTransform>();

        
        StartCoroutine(InitializeBarWidthSafely());

        InitializeHUD();

        if (shortRestButton != null)
            shortRestButton.onClick.AddListener(OnShortRestPressed);
    }


    public void SetTarget(CharacterStats newStats)
    {
        if (playerStats != null)
        {
            
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnStatsInitialized -= InitializeHUD;
            playerStats.OnMovementChanged -= UpdateMovementText;
        }

        playerStats = newStats;

        if (playerStats != null)
        {
            
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnStatsInitialized += InitializeHUD;
            playerStats.OnMovementChanged += UpdateMovementText;
        }

        
        InitializeHUD();
    }


    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnStatsInitialized -= InitializeHUD;
            playerStats.OnMovementChanged -= UpdateMovementText;
            playerStats.OnStatsInitialized -= UpdateSpellSlotPanels;

        }

        if (shortRestButton != null)
            shortRestButton.onClick.RemoveListener(OnShortRestPressed);
    }

    void InitializeHUD()
    {
        if (playerStats == null) return;

        if (playerPortrait != null && playerStats.characterPortrait != null)
            playerPortrait.sprite = playerStats.characterPortrait;

        UpdateHealthBar();
        UpdateMovementText();

        GenerateSpellSlots();
        UpdateActionUI();
        UpdateSpellSlotUI();
        UpdateSpellSlotPanels();
    }

    void UpdateHealthBar()
    {
        if (playerStats == null || healthFill == null || healthNumText == null)
            return;

        float healthPercent = Mathf.Clamp01((float)playerStats.currentHealth / playerStats.maxHealth);

       
        RectTransform mainRT = healthFill.GetComponent<RectTransform>();
        float newWidth = maxBarWidth * healthPercent;

        
        mainRT.DOSizeDelta(
            new Vector2(newWidth, mainRT.sizeDelta.y),
            0.25f
        ).SetEase(Ease.OutCubic);

        
        healthNumText.text = $"{playerStats.currentHealth} / {playerStats.maxHealth}";

        // DAMAGE FLASH only if losing HP 
        if (healthFill.fillAmount > healthPercent)
        {
            // Kill previous flash
            flashTween?.Kill();

            // Flash color and return to white
            flashTween = healthFill.DOColor(damageFlashColor, flashDuration)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }

        // CHIP DAMAGE BAR
        if (chipHealthFill != null)
        {
            RectTransform chipRT = chipHealthFill.GetComponent<RectTransform>();
            float chipWidth = chipRT.sizeDelta.x;

            // If chip bar is ahead, animate it down to the new value
            if (chipWidth > newWidth)
            {
                chipTween?.Kill();

                chipTween = DOVirtual.DelayedCall(chipDelay, () =>
                {
                    chipRT.DOSizeDelta(
                        new Vector2(newWidth, chipRT.sizeDelta.y),
                        chipSpeed
                    ).SetEase(Ease.OutCubic);

                });
            }
            else
            {
                // If healing → snap chip bar up immediately
                chipRT.sizeDelta = new Vector2(newWidth, chipRT.sizeDelta.y);
            }
        }

        // LOW HEALTH PULSE under 30% 
        float lowPercent = playerStats.currentHealth / (float)playerStats.maxHealth;

        if (lowPercent < 0.30f)
        {
            if (lowHealthTween == null)
            {
                lowHealthTween = healthFill.transform.DOScale(1.05f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.OutSine);
            }
        }
        else
        {
            // Stop pulsing if HP recovered
            lowHealthTween?.Kill();
            lowHealthTween = null;
            healthFill.transform.localScale = Vector3.one;
        }

        
        healthFill.fillAmount = healthPercent;
    }



    void UpdateMovementText()
    {
        if (movementText == null || playerStats == null) return;

        if (isInCombat)
        {
            movementText.text = $"{playerStats.currentMovement:0.00}m / {playerStats.maxMovement:0.00}m";
        }
        else
        {
           
            movementText.text = $"{playerStats.maxMovement:0.0}m";
        }

    }

    public void SetCombatState(bool inCombat)
    {
        isInCombat = inCombat;
        UpdateMovementText();

        if (shortRestButton != null)
            shortRestButton.interactable = !inCombat;
    }

    // Action UI
    public void UpdateActionUI()
    {
        if (playerStats == null) return;

        if (actionIcon != null)
            actionIcon.sprite = playerStats.hasAction ? actionAvailableSprite : actionUsedSprite;

        if (bonusActionIcon != null)
            bonusActionIcon.sprite = playerStats.hasBonusAction ? bonusAvailableSprite : bonusUsedSprite;
    }

    // Spell Slots
    void GenerateSpellSlots()
    {
        // Clear existing slots
        foreach (Transform child in level1SlotPanel)
            Destroy(child.gameObject);
        foreach (Transform child in level2SlotPanel)
            Destroy(child.gameObject);

        level1Slots.Clear();
        level2Slots.Clear();

        // Generate Level 1 slots
        for (int i = 0; i < playerStats.maxLevel1Slots; i++)
        {
            GameObject slot = Instantiate(spellSlotPrefab, level1SlotPanel);
            Image img = slot.GetComponent<Image>();
            img.sprite = slotAvailableSprite;
            level1Slots.Add(img);
        }

        // Generate Level 2 slots
        for (int i = 0; i < playerStats.maxLevel2Slots; i++)
        {
            GameObject slot = Instantiate(spellSlotPrefab, level2SlotPanel);
            Image img = slot.GetComponent<Image>();

            
            if (slotAvailableSpriteII != null)
                img.sprite = slotAvailableSpriteII;
            else
                img.sprite = slotAvailableSprite;

            level2Slots.Add(img);
        }
    }

    public void UpdateSpellSlotUI()
    {
        for (int i = 0; i < level1Slots.Count; i++)
        {
            if (i < playerStats.currentLevel1Slots)
                level1Slots[i].sprite = slotAvailableSprite;
            else
                level1Slots[i].sprite = slotUsedSprite;
        }

        for (int i = 0; i < level2Slots.Count; i++)
        {
            if (i < playerStats.currentLevel2Slots)
                level2Slots[i].sprite = slotAvailableSprite;
            else
                level2Slots[i].sprite = slotUsedSprite;
        }
    }

    // Short Rest
    public void OnShortRestPressed()
    {
        if (playerStats == null) return;

        // Limit to 3 uses
        if (shortRestsUsed >= maxShortRests)
        {
            shortRestButton.interactable = false;
            return;
        }

        shortRestsUsed++;

        // Restore spell slots + health for the WHOLE party
        var party = FindFirstObjectByType<PlayerPartyController>();
        if (party != null)
        {
            foreach (var member in party.partyMembers)
            {
                if (member == null) continue;

                CharacterStats stats = member.GetComponent<CharacterStats>();
                if (stats == null) continue;

                // Heal to full
                stats.SetCurrentHealth(stats.maxHealth);

                // REVIVE WITHOUT SPAWNING THEM:
                if (stats.currentHealth <= 0)
                {
                    stats.currentHealth = stats.maxHealth;
                }

                // Restore movement
                stats.ResetMovement();

                // Restore spell slots
                stats.RestoreAllSpellSlots();
            }
        }

        // Update UI for current character
        UpdateHealthBar();
        UpdateMovementText();
        UpdateSpellSlotUI();

        Debug.Log("Short Rest: Party fully restored.");

        // Disable after 3 rests
        if (shortRestsUsed >= maxShortRests)
            shortRestButton.interactable = false;
    }

    void UpdateSpellSlotPanels()
    {
        if (playerStats == null) return;

        bool hasLevel1Slots = playerStats.maxLevel1Slots > 0;
        bool hasLevel2Slots = playerStats.maxLevel2Slots > 0;

        if (spellSlot1Container != null)
            spellSlot1Container.SetActive(hasLevel1Slots);

        if (spellSlot2Container != null)
            spellSlot2Container.SetActive(hasLevel2Slots);
    }

    public void RefreshSkillBar(CharacterStats stats)
    {
        if (skillBarParent == null || stats == null) return;

        AbilityLoadout loadout = stats.GetComponent<AbilityLoadout>();
        var slots = skillBarParent.GetComponentsInChildren<SkillSlot>(true);

        if (loadout == null)
        {
            // Clear all slots if no loadout
            foreach (var slot in slots)
            {
                slot.assignedAbility = null;
                if (slot.iconImage == null)
                    slot.iconImage = slot.GetComponent<Image>();

                if (slot.iconImage != null)
                {
                    slot.iconImage.sprite = null;
                    slot.iconImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
            return;
        }

        Ability[] abilities = loadout.abilities;

        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlot uiSlot = slots[i];

            if (uiSlot.iconImage == null)
                uiSlot.iconImage = uiSlot.GetComponent<Image>();

            Ability ability = (abilities != null && i < abilities.Length) ? abilities[i] : null;

            uiSlot.assignedAbility = ability;

            if (ability != null && ability.abilityIcon != null && uiSlot.iconImage != null)
            {
                uiSlot.iconImage.sprite = ability.abilityIcon;
                uiSlot.iconImage.color = Color.white;
            }
            else if (uiSlot.iconImage != null)
            {
                uiSlot.iconImage.sprite = null;
                uiSlot.iconImage.color = new Color(1f, 1f, 1f, 0f); // transparent icon
            }
        }
    }
    private IEnumerator InitializeBarWidthSafely()
    {
        // Wait one frame so Unity calculates UI layout
        yield return null;

        if (healthParentRT != null)
            maxBarWidth = healthParentRT.rect.width;

        // Safety fallback if something still gives 0
        if (maxBarWidth <= 0f)
            maxBarWidth = 300f; // fallback so animation never breaks
    }



}
