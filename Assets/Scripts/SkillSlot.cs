using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public int slotNumber;
    public Image iconImage;
    public Ability assignedAbility;

    private AbilityExecutor executor;
    private BattleStateManager battle;

    private CharacterStats playerStats;
    private bool isUsable = true;
    void Start()
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        executor = FindFirstObjectByType<AbilityExecutor>();
        battle = FindFirstObjectByType<BattleStateManager>();

        var party = FindFirstObjectByType<PlayerPartyController>();
        if (party != null)
            playerStats = party.GetActiveStats();
    }


    void Update()
    {
        RefreshUsability();

        if (Input.GetKeyDown(KeyCode.Alpha0 + slotNumber))
            TryUse();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TryUse();
    }

    void TryUse()
    {
        if (assignedAbility == null)
            return;

        if (battle != null && !battle.isBattleActive)
            return;

        RefreshUsability();

        // Ability disabled?
        if (!isUsable)
        {
            // Which message?
            if (assignedAbility.maxUsesPerBattle > 0 &&
                assignedAbility.usesThisBattle >= assignedAbility.maxUsesPerBattle)
            {
                executor.ShowNoUsesLeftMessage();
            }
            else if (assignedAbility.usesSpellSlot)
            {
                executor.ShowNoSpellSlotsMessage();
            }

            return;
        }

        // Usable → play click + execute normally
        PlayClickPop();
        executor.ExecuteAbility(assignedAbility);
    }


    public void PlayClickPop()
    {
        iconImage.transform
            .DOPunchScale(Vector3.one * 0.1f, 0.2f, 6, 0.6f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (assignedAbility == null) return;

        Vector2 mousePos = Input.mousePosition;
        AbilityTooltipUI.Get()?.Show(assignedAbility, mousePos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AbilityTooltipUI.Get()?.Hide();
    }

    private void RefreshUsability()
    {
        isUsable = true;

        if (assignedAbility == null)
            return;

        // Check uses-per-battle
        if (assignedAbility.maxUsesPerBattle > 0 &&
            assignedAbility.usesThisBattle >= assignedAbility.maxUsesPerBattle)
        {
            isUsable = false;
        }

        // Check spell slots
        if (isUsable && assignedAbility.usesSpellSlot)
        {
            if (playerStats != null &&
                !playerStats.HasSpellSlots(assignedAbility.spellLevel, assignedAbility.slotCost))
            {
                isUsable = false;
            }
        }

        // Apply gray-out
        iconImage.color = isUsable ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
    }

}
