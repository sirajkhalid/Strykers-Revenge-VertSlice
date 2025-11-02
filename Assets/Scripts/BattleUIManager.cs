using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("References")]
    public CharacterStats playerStats;
    public Image playerPortrait;
    public Image healthFill;
    public TMP_Text healthNumText;

    private float maxBarWidth = 541f;

    void Start()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthBar;
            UpdateBattleUI();
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged -= UpdateHealthBar;
    }

    void UpdateBattleUI()
    {
        if (playerStats == null) return;

        if (playerPortrait != null && playerStats.characterPortrait != null)
            playerPortrait.sprite = playerStats.characterPortrait;

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (playerStats == null || healthFill == null || healthNumText == null) return;

        float healthPercent = Mathf.Clamp01((float)playerStats.currentHealth / playerStats.maxHealth);
        RectTransform rt = healthFill.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(maxBarWidth * healthPercent, rt.sizeDelta.y);

        healthNumText.text = $"{playerStats.currentHealth} / {playerStats.maxHealth}";
    }

    public void TakeDamage(int amount)
    {
        if (playerStats == null) return;
        playerStats.SetCurrentHealth(playerStats.currentHealth - amount);
    }
}
