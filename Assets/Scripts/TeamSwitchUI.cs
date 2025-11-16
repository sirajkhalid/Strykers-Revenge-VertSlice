using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TeamSwitchUI : MonoBehaviour
{
    [Header("Portrait Images")]
    public Image Z_Portrait;
    public Image X_Portrait;
    public Image C_Portrait;
    public Image V_Portrait;

    [Header("Health Text")]
    public TMP_Text Z_HealthText;
    public TMP_Text X_HealthText;
    public TMP_Text C_HealthText;
    public TMP_Text V_HealthText;

    [Header("Hover Behavior")]
    public bool showHealthOnHover = true;
    private int hoveredIndex = -1;

    [Header("Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f);
    public Color deadColor = new Color(0.3f, 0.3f, 0.3f);

    private PlayerPartyController party;

    IEnumerator Start()
    {
        yield return null;

        party = FindFirstObjectByType<PlayerPartyController>();
        RefreshDisplay();

        yield return new WaitForSeconds(0.05f);
        RefreshDisplay();
    }



    public void RefreshDisplay()
    {
        if (party == null || party.partyMembers.Count == 0) return;

        UpdateSlot(0, Z_Portrait, Z_HealthText);
        UpdateSlot(1, X_Portrait, X_HealthText);
        UpdateSlot(2, C_Portrait, C_HealthText);
        UpdateSlot(3, V_Portrait, V_HealthText);
    }

    void UpdateSlot(int index, Image portrait, TMP_Text healthText)
    {
        if (index >= party.partyMembers.Count)
        {
            portrait.enabled = false;
            healthText.text = "";
            return;
        }

        GameObject obj = party.partyMembers[index];
        CharacterStats stats = obj.GetComponent<CharacterStats>();

        portrait.enabled = true;
        portrait.sprite = stats.characterPortrait;

        bool isDead = stats.currentHealth <= 0;
        bool isSelected = party.activeIndex == index;
        bool isHovered = hoveredIndex == index;

        if (isDead)
        {
            portrait.color = deadColor;
            healthText.text = "Knocked Out";
            healthText.gameObject.SetActive(true);
            return;
        }

        portrait.color = isSelected ? activeColor : inactiveColor;

        bool showHP = isSelected || isHovered || !showHealthOnHover;

        healthText.gameObject.SetActive(showHP);
        if (showHP)
            healthText.text = $"{stats.currentHealth}/{stats.maxHealth}";
    }

    public void OnPortraitHover(int index)
    {
        hoveredIndex = index;
        RefreshDisplay();
    }

    public void OnPortraitExit(int index)
    {
        if (hoveredIndex == index)
            hoveredIndex = -1;

        RefreshDisplay();
    }

    public void ClickPortrait(int index)
    {
        var battle = FindFirstObjectByType<BattleStateManager>();
        if (battle != null && battle.isBattleActive)
            return; // no switching in battle

        if (party == null)
            party = FindFirstObjectByType<PlayerPartyController>();

        if (party != null)
            party.SwitchTo(index);

    }
}
