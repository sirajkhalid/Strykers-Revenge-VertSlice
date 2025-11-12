using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("References")]
    public CharacterStats playerStats;
    public Image playerPortrait;
    public Image healthFill;
    public TMP_Text healthNumText;
    public TMP_Text movementText;

    [Header("Settings")]
    public float maxBarWidth = 541f;
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

        InitializeHUD();

        if (shortRestButton != null)
            shortRestButton.onClick.AddListener(OnShortRestPressed);
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
        if (playerStats == null || healthFill == null || healthNumText == null) return;

        float healthPercent = Mathf.Clamp01((float)playerStats.currentHealth / playerStats.maxHealth);
        RectTransform rt = healthFill.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(maxBarWidth * healthPercent, rt.sizeDelta.y);

        healthNumText.text = $"{playerStats.currentHealth} / {playerStats.maxHealth}";
    }

    void UpdateMovementText()
    {
        if (movementText == null || playerStats == null) return;

        if (isInCombat)
            movementText.text = $"{playerStats.currentMovement:0.00}m / {playerStats.maxMovement:0.00}m";
        else
            movementText.text = $"{playerStats.maxMovement:0.0}m";
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

            // Use the darker Level II spell slot sprite if assigned, otherwise fall back to normal
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

        playerStats.currentLevel1Slots = playerStats.maxLevel1Slots;
        playerStats.currentLevel2Slots = playerStats.maxLevel2Slots;

        UpdateSpellSlotUI();
        Debug.Log("Short Rest: Spell slots restored.");
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
}
