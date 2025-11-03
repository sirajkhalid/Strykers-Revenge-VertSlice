using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<CharacterStats>();
    }

    void Start()
    {
        // Subscribe to stat changes
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
        }
    }

    // Called when player stats initialize
    void InitializeHUD()
    {
        if (playerStats == null) return;

        if (playerPortrait != null && playerStats.characterPortrait != null)
            playerPortrait.sprite = playerStats.characterPortrait;

        UpdateHealthBar();
        UpdateMovementText();
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
        {
            movementText.text = $"{playerStats.currentMovement:0.00}m / {playerStats.maxMovement:0.00}m";
        }
        else
        {
            movementText.text = $"{playerStats.maxMovement:0.0}m";
        }
    }

    // This will be called by BattleStateManager when combat starts/ends
    public void SetCombatState(bool inCombat)
    {
        isInCombat = inCombat;
        UpdateMovementText();
    }
}
