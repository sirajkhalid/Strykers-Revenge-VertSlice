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

    [Header("Health Fill Bars (red)")]
    public Image Z_HealthFill;
    public Image X_HealthFill;
    public Image C_HealthFill;
    public Image V_HealthFill;

    [Header("Hover Behavior")]
    public bool showHealthOnHover = true;
    private int hoveredIndex = -1;

    [Header("Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f);
    public Color deadColor = new Color(0.3f, 0.3f, 0.3f);

    private PlayerPartyController party;
    public Transform PortraitContainer;
    public System.Action<int> onHealPortraitClicked;

    
    private float zMaxWidth;
    private float xMaxWidth;
    private float cMaxWidth;
    private float vMaxWidth;

    IEnumerator Start()
    {
        yield return null;

        party = FindFirstObjectByType<PlayerPartyController>();

       
        if (Z_HealthFill != null) zMaxWidth = Z_HealthFill.rectTransform.sizeDelta.x;
        if (X_HealthFill != null) xMaxWidth = X_HealthFill.rectTransform.sizeDelta.x;
        if (C_HealthFill != null) cMaxWidth = C_HealthFill.rectTransform.sizeDelta.x;
        if (V_HealthFill != null) vMaxWidth = V_HealthFill.rectTransform.sizeDelta.x;

        
        if (party != null)
        {
            foreach (var member in party.partyMembers)
            {
                var stats = member.GetComponent<CharacterStats>();
                if (stats != null)
                    stats.OnHealthChanged += RefreshDisplay;
            }
        }

        RefreshDisplay();
        yield return new WaitForSeconds(0.05f);
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (party == null || party.partyMembers.Count == 0) return;

        UpdateSlot(0, Z_Portrait, Z_HealthText, Z_HealthFill, zMaxWidth);
        UpdateSlot(1, X_Portrait, X_HealthText, X_HealthFill, xMaxWidth);
        UpdateSlot(2, C_Portrait, C_HealthText, C_HealthFill, cMaxWidth);
        UpdateSlot(3, V_Portrait, V_HealthText, V_HealthFill, vMaxWidth);
    }

    void UpdateSlot(int index, Image portrait, TMP_Text healthText, Image fillBar, float maxWidth)
    {
        if (index >= party.partyMembers.Count)
        {
            if (portrait != null) portrait.enabled = false;
            if (healthText != null) healthText.text = "";
            if (fillBar != null)
            {
                var rt = fillBar.rectTransform;
                rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y);
            }
            return;
        }

        GameObject obj = party.partyMembers[index];
        CharacterStats stats = obj.GetComponent<CharacterStats>();

        // Portrait
        portrait.enabled = true;
        portrait.sprite = stats.characterSquarePortrait;

        bool isDead = stats.currentHealth <= 0;
        bool isSelected = party.activeIndex == index;
        bool isHovered = hoveredIndex == index;

        if (isDead)
        {
            portrait.color = deadColor;
            healthText.gameObject.SetActive(true);
            healthText.text = "Dead";

            var fx = portrait.GetComponent<PartyPortraitFX>();
            if (fx != null)
                fx.PlayDeadEffect();

            
            if (fillBar != null)
            {
                var rt = fillBar.rectTransform;
                rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y);
            }
            return;
        }
        else
        {
            var fx = portrait.GetComponent<PartyPortraitFX>();
            if (fx != null)
                fx.ResetToNormal();
        }

        portrait.color = isSelected ? activeColor : inactiveColor;

        // Text
        healthText.gameObject.SetActive(true);
        healthText.text = $"{stats.currentHealth}/{stats.maxHealth}";

        // Bar width based on HP%
        if (fillBar != null && maxWidth > 0f)
        {
            float ratio = stats.maxHealth > 0
                ? (float)stats.currentHealth / stats.maxHealth
                : 0f;

            ratio = Mathf.Clamp01(ratio);

            RectTransform rt = fillBar.rectTransform;
            rt.sizeDelta = new Vector2(maxWidth * ratio, rt.sizeDelta.y);
        }
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
        // Heal-selection mode
        if (onHealPortraitClicked != null)
        {
            onHealPortraitClicked.Invoke(index);
            return;
        }

        // Block switching during battle
        var battle = FindFirstObjectByType<BattleStateManager>();
        if (battle != null && battle.isBattleActive)
            return;

        if (party == null)
            party = FindFirstObjectByType<PlayerPartyController>();

        if (party != null)
        {
            party.SwitchTo(index);

            var fx = party.partyMembers[index].GetComponentInChildren<PartyPortraitFX>();
            if (fx != null)
                fx.PlaySelectEffect();
        }
    }

    public void PlayPortraitSelectFX(int index)
    {
        if (PortraitContainer == null) return;
        if (index < 0 || index >= PortraitContainer.childCount) return;

        Transform portrait = PortraitContainer.GetChild(index);
        var fx = portrait.GetComponent<PartyPortraitFX>();
        if (fx != null)
            fx.PlaySelectEffect();
    }
}
