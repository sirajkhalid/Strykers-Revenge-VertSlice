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
}
