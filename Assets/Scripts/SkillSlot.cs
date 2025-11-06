using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerClickHandler
{
    public int slotNumber;                     // 1–9
    public Ability assignedAbility;            // Dragged in from SkillBook
    public Image iconImage;                    // The ability icon image
    public AbilityExecutor abilityExecutor;    // Global executor
    public BattleStateManager battleManager;   // Reference to check combat state

    private void Start()
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (abilityExecutor == null)
            abilityExecutor = FindFirstObjectByType<AbilityExecutor>();

        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleStateManager>();
        
        if (assignedAbility != null && iconImage != null)
            iconImage.sprite = assignedAbility.abilityIcon;
    }

    private void Update()
    {
        // Keyboard shortcut (1–9)
        if (Input.GetKeyDown(KeyCode.Alpha0 + slotNumber))
            TryUseAbility();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TryUseAbility();
    }

    void TryUseAbility()
    {
        if (assignedAbility == null)
        {
            Debug.Log($"{name}: No ability assigned.");
            return;
        }

        if (!battleManager.isBattleActive)
        {
            Debug.Log($"{name}: Cannot use {assignedAbility.abilityName} outside of battle!");
            return;
        }

        Debug.Log($"{name}: Using ability {assignedAbility.abilityName}");
        abilityExecutor.ExecuteAbility(assignedAbility);
    }
}
