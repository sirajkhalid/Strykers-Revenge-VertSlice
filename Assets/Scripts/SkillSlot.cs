using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour, IPointerClickHandler
{
    public int slotNumber;
    public Image iconImage;
    public Ability assignedAbility;

    private AbilityExecutor executor;
    private BattleStateManager battle;

    void Start()
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        executor = FindFirstObjectByType<AbilityExecutor>();
        battle = FindFirstObjectByType<BattleStateManager>();
    }

    void Update()
    {
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

        executor.ExecuteAbility(assignedAbility);
    }
}
